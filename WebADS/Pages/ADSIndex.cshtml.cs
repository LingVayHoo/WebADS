using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebADS.Models;
using WebADS.Models.MoySklad.Stock;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSIndexModel : PageModel
    {
        private readonly ILogger<ADSIndexModel> _logger;
        private readonly AuthService _authService;
        private readonly MyStorageAPIModel _myStorageAPIModel;
        private readonly ADSHandler _adshandler;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyStorageRequester _myStorageRequester;

        public Dictionary<string, string> Stores { get; set; } = new();

        [BindProperty]
        public string SelectedStore { get; set; } = "all"; // По умолчанию выбран "Все"

        [BindProperty]
        public string SelectedArticleJson { get; set; } // Для привязки JSON из FormData

        [BindProperty]
        public string Search { get; set; } = string.Empty;

        [BindProperty]
        public ProductModel SelectedProduct { get; set; } = new();

        [BindProperty]
        public ProductModel SelectedArticle { get; set; } = new();

        [BindProperty]
        public List<ProductModel> SearchResults { get; set; } = new();

        public ADSIndexModel(
            ILogger<ADSIndexModel> logger,
            AuthService authService,
            MyStorageAPIModel myStorageAPIModel,
            ADSHandler adshandler,
            UserManager<ApplicationUser> userManager,
            MyStorageRequester myStorageRequester)
        {
            _logger = logger;
            _authService = authService;
            _myStorageAPIModel = myStorageAPIModel;
            _adshandler = adshandler;
            _userManager = userManager;
            _myStorageRequester = myStorageRequester;
        }

        public async Task OnGetAsync()
        {
            var storesJson = HttpContext.Session.GetString("Stores");

            if (!string.IsNullOrEmpty(storesJson))
            {
                Stores = JsonConvert.DeserializeObject<Dictionary<string, string>>(storesJson) ?? new();
            }

            if (string.IsNullOrEmpty(storesJson) || Stores.Count < 3)
            {
                var user = await _userManager.GetUserAsync(User);
                var username = user?.MoySkladUsername ?? string.Empty;
                var token = user?.MoySkladToken ?? "default";


                Stores = await _authService.GetStorages(username, token);
                Stores["all"] = "all";

                // Сохраняем в Session
                HttpContext.Session.SetString("Stores", JsonConvert.SerializeObject(Stores));
            }
        }



        public async Task<JsonResult> OnPostSearchAsync()
        {
            if (string.IsNullOrEmpty(Search)) return new JsonResult(null);

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            var username = user?.UserName ?? string.Empty;

            SearchResults = await _myStorageAPIModel.GetADSProducts(Search, username);
            return new JsonResult(SearchResults.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                article = p.Article
            }).ToList());
        }
             

        private async Task<List<StockInfo>> GetStockInformation(ProductModel productModel)
        {
            if (productModel == null || string.IsNullOrEmpty(productModel.ProductHref))
                return new List<StockInfo>();

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);

            var username = user?.UserName ?? string.Empty;

            StockByStoreRoot? stock =
                await _myStorageAPIModel.GetSingleProductStockAllStore(productModel.ProductHref, username);
          

            if (stock == null || stock.Rows == null || stock.Rows.Count == 0)
                return new List<StockInfo>();

            List<StockInfo> stockInfo = new List<StockInfo>();

            var needed = stock.Rows.FirstOrDefault(n =>
                n.Meta != null &&
                !string.IsNullOrEmpty(n.Meta.Href) &&
                !string.IsNullOrEmpty(productModel.ProductHref) &&
                n.Meta.Href.Contains(productModel.ProductHref)
            );

            if (needed == null || needed.StockByStore == null || needed.StockByStore.Count == 0) 
                return new List<StockInfo>();

            foreach (var item in needed.StockByStore)
            {
                if (item == null) continue;
                stockInfo.Add(new StockInfo()
                {
                    StoreHref = item.Meta?.Href ?? string.Empty,
                    Name = item.Name,
                    Stock = (float)item.Stock,
                    Reserve = (float)item.Reserve
                });    
            }

            return stockInfo;
        }

        

        public async Task<JsonResult> OnPostGetAddressesAsync(string article)
        {
            var r = await _adshandler.GetAddresses(article);
            return new JsonResult(r);
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
    }

    public class ProductModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string FirstImageHref {  get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string ProductHref {  get; set; } = string.Empty;
        public List<StockInfo> StockInfoModel { get; set; } = new();
        public bool? IsWardrobe { get; set; }
    }

    public class StockInfo
    {
        public string StoreHref { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float Stock {  get; set; }
        public float Reserve { get; set; }
    }
}