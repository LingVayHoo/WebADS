using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System.Text.Json;
using WebADS.Models.MoySklad.Product;
using WebADS.Models.MoySklad.Stock;
using WebADS.Models.MoySklad.Storages;
using WebADS.Models.Token;
using WebADS.Pages;

namespace WebADS.Services
{
    public class MyStorageAPIModel
    {
        private MyStorageRequester _myStorageRequester;
        private MoySkladTokenHandler _mySkladTokenHandler;
        private MasterToken _masterToken;

        public MyStorageAPIModel(
            MyStorageRequester myStorageRequester, 
            MoySkladTokenHandler mySkladTokenHandler, 
            MasterToken masterToken)
        {
            _myStorageRequester = myStorageRequester;
            _mySkladTokenHandler = mySkladTokenHandler;
            _masterToken = masterToken;
        }

        public async Task<ProductModel?> GetProductAsync(string moySkladId, string identityUsername)
        {
            string url = $"https://api.moysklad.ru/api/remap/1.2/entity/product/{moySkladId}";
            var token = await GetTokenByID(identityUsername);

            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var json = await _myStorageRequester.GetRequestAsync(url, token);

                Product product = JsonConvert.DeserializeObject<Product>(json) ?? new Product();

                ProductModel productModel = new ProductModel()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Article = product.Article,
                    FirstImageHref = product.Images?.Meta?.Href ?? string.Empty,
                    ProductHref = product.Meta?.Href ?? string.Empty
                };

                return productModel;
            }
            catch (Exception)
            {
                return null;
            }

            
        }

        public async Task<List<ProductModel>> GetADSProducts(string searchData, string identityUsername)
        {
            ProductRoot root = new ProductRoot();
            List<ProductModel> results = new List<ProductModel>();

            if (searchData == "searchForAllocation")
            {
                string url = "https://api.moysklad.ru/api/remap/1.2/entity/product?filter=pathName=Базовые модели";

                root = await GetPaginatedDataAsync(url, identityUsername);
            }
            else
            {
                var searchResult = await GetSearchResultsAsync(searchData, identityUsername);
                if (searchResult == null) return results;

                root = JsonConvert.DeserializeObject<ProductRoot>(searchResult) ?? new ProductRoot();
            }

            if (root.Rows == null || root.Rows.Count == 0)
            {
                return results;
            }

            // Проход по массиву и заполнение словаря
            foreach (var row in root.Rows)
            {
                if (row == null) continue;
                //string id = row["id"]?.ToString() ?? string.Empty;
                //string name = row["name"]?.ToString() ?? string.Empty;
                //string article = row["article"]?.ToString() ?? string.Empty;

                var attribute = row.Attributes?.FirstOrDefault(t => t.Name == "Wardrobe");
                bool isWardrobe = attribute?.Value == "true" ? true : false;

                ProductModel productModel = new ProductModel()
                {
                    Id = row.Id,
                    Name = row.Name,
                    Article = row.Article,
                    FirstImageHref = row.Images?.Meta?.Href ?? string.Empty,
                    ProductHref = row.Meta?.Href ?? string.Empty,
                    IsWardrobe = isWardrobe
                };

                results.Add(productModel);
            }

            return results;

        }

        public async Task<string?> GetSearchResultsAsync(string searchData, string identityUsername)
        {
            string url = $"https://api.moysklad.ru/api/remap/1.2/entity/product?search={searchData}";

            var token = await GetTokenByID(identityUsername);

            if (string.IsNullOrEmpty(token)) return null;

            try
            {                
                return await _myStorageRequester.GetRequestAsync(url, token);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<ProductRoot> GetPaginatedDataAsync(string url, string identityUsername)
        {
            var token = await GetTokenByID(identityUsername);
            var allRows = new List<Product>();
            string? currentUrl = url;
            ProductRoot? root = null;
            while (!string.IsNullOrEmpty(currentUrl))
            {
                var json = await _myStorageRequester.GetRequestAsync(currentUrl, token);
                var currentPage = JsonConvert.DeserializeObject<ProductRoot>(json)
                                  ?? throw new Exception("Не удалось десериализовать JSON.");
                root ??= currentPage;
                if (currentPage.Rows != null)
                    allRows.AddRange(currentPage.Rows);
                currentUrl = currentPage.Meta?.NextHref;
            }
            if (root != null)
                root.Rows = allRows;
            return root!;
        }

        public async Task<string?> GetImageURL(string imagesHref, string identityUsername)
        {
            var token = await GetTokenByID(identityUsername);

            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                var imagesResponse = await _myStorageRequester.GetRequestAsync(imagesHref, token);
                var imagesJson = JsonDocument.Parse(imagesResponse);
                var imageUrlRequest = imagesJson.RootElement.GetProperty("rows")[0].GetProperty("meta").GetProperty("downloadHref").GetString();
                //imageUrl = GetImageUrlFromResponseAsync(imagesResponse);
                var image = await _myStorageRequester.GetResponseWithoutRedirectsAsync(imageUrlRequest, token);
                string imageUrl = GetImageUrlFromResponseAsync(image) ?? string.Empty;

                return imageUrl;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string? GetImageUrlFromResponseAsync(HttpResponseMessage response)
        {
            // Проверьте, содержит ли заголовок `Location`
            if (response.Headers.Location != null)
            {
                // Извлеките URL из заголовка `Location`
                string imageUrl = response.Headers.Location.ToString();
                return imageUrl;
            }

            // Если заголовок `Location` отсутствует, верните null или пустую строку
            return string.Empty;
        }

        public async Task<StockByStoreRoot?> GetSingleProductStockAllStore(string productHref, string identityUsername)
        {
            var token = await GetTokenByID(identityUsername);

            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                string url = $"https://api.moysklad.ru/api/remap/1.2/report/stock/bystore?filter=product={productHref}";

                string responce = await _myStorageRequester.GetRequestAsync(url, token);
                StockByStoreRoot stockByStoreRoot =
                    JsonConvert.DeserializeObject<StockByStoreRoot>(responce) ?? new StockByStoreRoot();

                return stockByStoreRoot;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<StockInfo>> GetStockInformation(ProductModel productModel, string identityUsername)
        {
            if (productModel == null || string.IsNullOrEmpty(productModel.ProductHref))
                return new List<StockInfo>();

            StockByStoreRoot? stock =
                await GetSingleProductStockAllStore(productModel.ProductHref, identityUsername);

            if (stock == null || stock.Rows == null || stock.Rows.Count == 0)
                return new List<StockInfo>(); ;

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

        public async Task<List<ShortStockItem>> GetShortStock()
        {
            string url = $"https://api.moysklad.ru/api/remap/1.2/report/stock/bystore/current?stockType=quantity";

            try
            {
                string responce = await _myStorageRequester.GetRequestAsync(url, _masterToken.Master_token);
                List<ShortStockItem> items = JsonConvert.DeserializeObject<List<ShortStockItem>>(responce) ?? [];

                return items;
            }
            catch (Exception)
            {
                return [];
            }

            
        }

        private async Task<string> GetTokenByID(string username)
        {
            var token = await _mySkladTokenHandler.GetMoySkladTokenFromDB(username);

            return token;
        }
    }
}
