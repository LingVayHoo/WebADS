using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using WebADS.Models;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class AdsSLMModel : PageModel
    {
        private readonly ADSHandler _adshandler;
        private readonly AuthService _authService;
        private MyStorageAPIModel _myStorageAPIModel;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ArticleParametersHandler _articleParametersHandler;
        private readonly IMemoryCache _cache;

        public AdsSLMModel(
            MyStorageAPIModel myStorageAPIModel, 
            ADSHandler adshandler, 
            UserManager<ApplicationUser> userManager, 
            AuthService authService, 
            ArticleParametersHandler articleParametersHandler, 
            IMemoryCache cache)
        {
            _myStorageAPIModel = myStorageAPIModel;
            _adshandler = adshandler;
            _userManager = userManager;
            _authService = authService;
            _articleParametersHandler = articleParametersHandler;
            _cache = cache;
        }

        public string MoySkladId { get; set; }

        public Dictionary<string, string> Stores { get; set; } = new();

        [BindProperty]
        public string SelectedStore { get; set; } = "all";

        [BindProperty]
        public string SelectedArticleJson { get; set; }

        public async Task OnGetAsync(string moySkladId)
        {
            MoySkladId = moySkladId;

            await UpdateStoresData();
            //await _articleParametersHandler.UpdateAWS();
        }

        public async Task<JsonResult> OnPostGetAddressesAsync(string article)
        {
            var r = await _adshandler.GetAddresses(article);
            return new JsonResult(r);
        }

        public async Task<JsonResult> OnPostGetHistoryByArticleAsync(string article)
        {
            var r = await _adshandler.GetHistoryByArticle(article);
            return new JsonResult(r);
        }

        public async Task<JsonResult> OnPostArticleParamsAsync(string moySkladId)
        {
            var p = await _articleParametersHandler.GetParameters(moySkladId);
            return new JsonResult(p);
        }

        public async Task<JsonResult> OnPostUpdateArticleParamsAsync([FromBody] ArticleParameters newParameters)
        {
            bool responce = await _articleParametersHandler.Update(newParameters);

            return new JsonResult(new { success = true });
        }

        public async Task<JsonResult> OnPostCreateAddressAsync([FromBody] AddressHistory addressHistory)
        {
            try
            {
                // Получаем текущего пользователя
                var user = await _userManager.GetUserAsync(User);
                var username = user?.UserName ?? string.Empty;

                addressHistory.ChangedBy = user?.UserName ?? string.Empty;

                await _adshandler.PostAddressDBModel(addressHistory, username);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> OnPostPutAddressAsync([FromBody] AddressHistory addressHistory)
        {
            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            addressHistory.ChangedBy = user?.UserName ?? string.Empty;

            var username = user?.UserName ?? string.Empty;

            var r = await _adshandler.PutAddressDBModel(addressHistory, username);
            return new JsonResult(r);
        }

        public async Task<JsonResult> OnPostDeleteAddressAsync([FromBody] DeleteAddressRequest guidIdModel)
        {
            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            var ChangedBy = user?.UserName ?? string.Empty;

            if (Guid.TryParse(guidIdModel.guidId, out Guid guid))
            {
                var r = await _adshandler.DeleteAddressDBModel(guid, ChangedBy);
                return new JsonResult(true);
            }          
            return new JsonResult(false);
        }

        public async Task<JsonResult> OnPostSelectProductAsync(string moySkladId)
        {
            if (string.IsNullOrEmpty(moySkladId)) return new JsonResult(null);

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            var username = user?.UserName ?? string.Empty;

            var productModel = await _myStorageAPIModel.GetProductAsync(moySkladId, username);

            if (productModel == null) return new JsonResult(null);

            if (!string.IsNullOrEmpty(productModel.FirstImageHref))
            {
                productModel.ImageURL = 
                    await _myStorageAPIModel.GetImageURL(productModel.FirstImageHref, username) ??
                    string.Empty;
            }

            productModel.StockInfoModel = await _myStorageAPIModel.GetStockInformation(productModel, username);

            return new JsonResult(productModel);
        }

        public async Task<JsonResult> OnPostSelectedStoreAsync()
        {
            // Десериализация JSON
            var selectedArticle = JsonConvert.DeserializeObject<ProductModel>(SelectedArticleJson);


            if (selectedArticle == null)
            {
                return new JsonResult(new
                {
                    stock = 0,
                    reserve = 0
                });
            }

            var stockInfo = await GetStockInfoForStore(selectedArticle.StockInfoModel, SelectedStore);

            return new JsonResult(new
            {
                stock = stockInfo.Stock,
                reserve = stockInfo.Reserve
            });
        }

        public async Task<(float TotalStock, float TotalReserve)> CalculateTotalStockInfo(List<StockInfo> stockInfo)
        {
            await UpdateStoresData();
            float totalStock = stockInfo.Sum(s => s.Stock);
            float totalReserve = stockInfo.Sum(s => s.Reserve);
            return (totalStock, totalReserve);
        }

        public async Task<(float Stock, float Reserve)> GetStockInfoForStore(List<StockInfo> stockInfo, string selectedStore)
        {
            if (selectedStore == "all")
            {
                return (await CalculateTotalStockInfo(stockInfo));
            }

            await UpdateStoresData();
            var storeStockInfo = stockInfo.FirstOrDefault(s => GetId(s.StoreHref) == selectedStore);
            if (storeStockInfo != null)
            {
                return (storeStockInfo.Stock, storeStockInfo.Reserve);
            }
            return (0, 0); // Если склад не найден
        }

        public string GetId(string href)
        {
            var split = href.Split('/');
            if (split == null || split.Length == 0)
                return string.Empty;

            var finalVar = split[split.Length - 1].Split('?');

            if (finalVar == null || finalVar.Length == 0)
                return string.Empty;

            return finalVar[0];
        }

        private async Task UpdateStoresData()
        {
            try
            {
                // Получаем текущего пользователя
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "guest"; // Или другое уникальное поле для пользователя

                // Проверяем, есть ли данные в кэше для текущего пользователя
                if (_cache.TryGetValue(userId, out Dictionary<string, string> cachedStores))
                {
                    Stores = cachedStores;
                    return;
                }

                // Если данных нет, загружаем их
                var username = user?.MoySkladUsername ?? string.Empty;
                var token = user?.MoySkladToken ?? "default";

                Stores = await _authService.GetStorages(username, token);
                Stores["all"] = "all";

                // Кэшируем данные для этого пользователя
                _cache.Set(userId, Stores, TimeSpan.FromMinutes(30)); // Кэшируем на 30 минут
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении данных складов: {ex.Message}");
            }
        }


        //private async Task UpdateStoresData()
        //{
        //    var storesJson = HttpContext.Session.GetString("Stores");

        //    if (!string.IsNullOrEmpty(storesJson))
        //    {
        //        Stores = JsonConvert.DeserializeObject<Dictionary<string, string>>(storesJson) ?? new();
        //    }
        //    else
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        var username = user?.MoySkladUsername ?? string.Empty;

        //        Stores = await _authService.GetStorages(username);
        //        Stores["all"] = "all";

        //        // Сохраняем в Session
        //        HttpContext.Session.SetString("Stores", JsonConvert.SerializeObject(Stores));
        //    }
        //}

        public class StoreSelectionRequest
        {
            public string SelectedStore { get; set; }
            public string SelectedArticleJson { get; set; }
        }

        public class DeleteAddressRequest
        {
            public string guidId { get; set; }
        }
    }
}
