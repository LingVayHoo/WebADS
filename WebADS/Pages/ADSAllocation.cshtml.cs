using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebADS.Data;
using WebADS.Models;
using WebADS.Models.MoySklad.Stock;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSAllocationModel : PageModel
    {
        private MyStorageAPIModel _myStorageAPIModel;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthService _authService;
        private readonly ADSContext _adsContext;

        public ADSAllocationModel(
            MyStorageAPIModel myStorageAPIModel,
            UserManager<ApplicationUser> userManager,
            AuthService authService,
            ADSContext adsContext)
        {
            _myStorageAPIModel = myStorageAPIModel;
            _userManager = userManager;
            _authService = authService;
            _adsContext = adsContext;
        }

        [BindProperty]
        public string SelectedStoreFrom { get; set; } = "-- �������� �������� --";

        [BindProperty]
        public string SelectedStoreTo { get; set; } = "-- �������� �������� --";

        public Dictionary<string, string> Stores { get; set; } = new();

        public async void OnGet()
        {
            await UpdateStoresData();
        }

        public async Task<JsonResult> OnPostGetAllocationInfoAsync(
            string storeFrom, string storeTo, float minQty)
        {
            var user = await _userManager.GetUserAsync(User);
            var username = user?.UserName ?? string.Empty;

            // �������� ������ ������� �� ������ � ��������� ��������
            List<ShortStockItem> storesStock = await _myStorageAPIModel.GetShortStock();
            List<ArticleParameters> articlesParameters = await _adsContext.articleparameters.ToListAsync();
            List<ProductModel> assortmentInfo = 
                await _myStorageAPIModel.GetADSProducts("searchForAllocation", username);

            // ��������� ��� �������� ���������� �� �����������������
            List<AllocationInfo> allocationInfoList = new List<AllocationInfo>();

            // ��������� ������ ��� ������ storeTo
            var storeToStocks = storesStock.Where(stock => stock.StoreId == storeTo);

            foreach (var stock in storeToStocks)
            {
                // ������� ��������� �������� �� ���������� ��������������
                var articleParam = articlesParameters.FirstOrDefault(a => a.ProductID == stock.AssortmentId);
                if (articleParam != null)
                {
                    // ���� ���������� �� ������ storeTo ������ ������ �� ������� ��� ������ 3 ������
                    if (stock.Quantity < articleParam.AWS ||
                        stock.Quantity < articleParam.MinSalesQty ||
                        stock.Quantity < articleParam.MultipackQty ||
                        stock.Quantity < minQty)
                    {
                        // ��������� ������� ������� �������� �� ������ storeFrom
                        var storeFromStock = storesStock
                            .FirstOrDefault(s => s.StoreId == storeFrom && s.AssortmentId == stock.AssortmentId);

                        var ArticleInfo = assortmentInfo.FirstOrDefault(a => a.Id == stock.AssortmentId);

                        if (storeFromStock != null)
                        {
                            // ��������� ������ AllocationInfo � ��������� � ���������
                            allocationInfoList.Add(new AllocationInfo
                            {
                                Article = ArticleInfo?.Article ?? string.Empty,
                                ArticleName = ArticleInfo?.Name ?? string.Empty,
                                FromStoreQty = storeFromStock.Quantity,
                                ToStoreQty = stock.Quantity,
                                AWS = articleParam.AWS,
                                Multipack = articleParam.MultipackQty,
                                IsWardrobe = ArticleInfo?.IsWardrobe ?? false
                            });
                        }
                    }
                }
            }
            
            return new JsonResult(allocationInfoList);
        }


        private async Task UpdateStoresData()
        {
            var storesJson = HttpContext.Session.GetString("Stores");

            if (!string.IsNullOrEmpty(storesJson))
            {
                Stores = JsonConvert.DeserializeObject<Dictionary<string, string>>(storesJson) ?? new();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                var username = user?.MoySkladUsername ?? string.Empty;
                var token = user?.MoySkladToken ?? "default";

                Stores = await _authService.GetStorages(username, token);
                Stores["all"] = "all";

                // ��������� � Session
                HttpContext.Session.SetString("Stores", JsonConvert.SerializeObject(Stores));
            }
        }

        public class AllocationInfo
        {
            public string Article { get; set; } = string.Empty;
            public string ArticleName { get; set; } = string.Empty;
            public float FromStoreQty { get; set; }
            public float ToStoreQty { get; set; }
            public float AWS {  get; set; }
            public float Multipack {  get; set; }
            public bool IsWardrobe { get; set; }
        }
    }
}
