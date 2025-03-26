using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebADS.Data;
using WebADS.Models;
using WebADS.Models.Token;

namespace WebADS.Services
{
    public class ArticleParametersHandler
    {
        private readonly MyStorageRequester _requester;
        private readonly MasterToken _masterToken;
        private readonly ADSContext _context;

        public ArticleParametersHandler(MyStorageRequester requester, MasterToken masterToken, ADSContext context)
        {
            _requester = requester;
            _masterToken = masterToken;
            _context = context;
        }

        public async Task<ArticleParameters> GetParameters(string productID)
        {
            var existingArticle = await _context.articleparameters.
                FirstOrDefaultAsync(a => a.ProductID == productID);

            return existingArticle ?? new ArticleParameters();
        }

        public async Task Create(ArticleParameters articleParameters)
        {
            // Пробуем найти и обновить запись
            bool r = await Update(articleParameters);

            if (!r)
            {
                ArticleParameters article = new ArticleParameters()
                {
                    Id = Guid.NewGuid(),
                    ProductID = articleParameters.ProductID,
                    AWS = (float)articleParameters.AWS,
                    SalesMethod = articleParameters.SalesMethod,
                    MinSalesQty = articleParameters.MinSalesQty,
                    MultipackQty = articleParameters.MultipackQty,
                    PalletQty = articleParameters.PalletQty
                };

                _context.articleparameters.Add(article);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Update(ArticleParameters articleParameters)
        {
            var existingArticle = await _context.articleparameters.
                FirstOrDefaultAsync(a => a.ProductID == articleParameters.ProductID);

            if (existingArticle != null)
            {                
                existingArticle.AWS = articleParameters.AWS;
                existingArticle.SalesMethod = articleParameters.SalesMethod;
                existingArticle.MinSalesQty = articleParameters.MinSalesQty;
                existingArticle.MultipackQty = articleParameters.MultipackQty;
                existingArticle.PalletQty = articleParameters.PalletQty;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task Delete(ArticleParameters articleParameters)
        {
            var existingArticle = await _context.articleparameters.
                FirstOrDefaultAsync(a => a.ProductID == articleParameters.ProductID);

            if (existingArticle != null)
            {
                _context.articleparameters.Remove(existingArticle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> UpdateAWS()
        {
            try
            {
                var data = await GetAms();
                var uniqueArticles = data
                    .DistinctBy(a => a.ProductID)
                    .ToList();

                var productIds = uniqueArticles.Select(d => d.ProductID).ToList();

                Console.WriteLine($"После формирования - ID: {productIds[0]}");

                // Загружаем все записи, которые уже есть в базе одним запросом
                var existingArticles = await _context.articleparameters
                    .Where(a => productIds.Contains(a.ProductID))
                    .ToDictionaryAsync(a => a.ProductID);

                var newArticles = new List<ArticleParameters>();

                foreach (var item in uniqueArticles)
                {
                    if (existingArticles.TryGetValue(item.ProductID, out var article))
                    {
                        article.AWS = (float)item.AMS / 4;
                    }
                    else
                    {
                        try
                        {
                            newArticles.Add(new ArticleParameters
                            {
                                Id = Guid.NewGuid(),
                                ProductID = item.ProductID,
                                AWS = (float)item.AMS / 4,
                                SalesMethod = "N/A",
                                MinSalesQty = 1,
                                MultipackQty = 1,
                                PalletQty = 1
                            });
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        
                    }
                }

                // Логируем количество новых записей
                Console.WriteLine($"Количество новых записей для добавления: {newArticles.Count}");

                if (newArticles.Count > 0)
                {
                    await _context.articleparameters.AddRangeAsync(newArticles);
                }

                await _context.SaveChangesAsync();
                return "Успешно!";
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка: {ex.Message}");
                return ex.ToString();
            }
        }



        private async Task<ArticleProfitability[]> GetAms()
        {
            DateTime date = DateTime.Now;
            string fromDate = date.AddMonths(-4).ToString("yyyy-MM-dd");
            string toDate = date.ToString("yyyy-MM-dd");
            string storeID = "8eecadfd-0367-11ed-0a80-0b430036e29c"; // Временно, ЧернаяРечка, так как история есть только там

            //string url = $"https://localhost:44389/api/ArticleForecast/ams?" +
            //    $"fromDate={fromDate}&toDate={toDate}&store={storeID}";
            string url = $"https://valyashki.ru/forecastapi/api/ArticleForecast/ams?" +
                $"fromDate={fromDate}&toDate={toDate}&store={storeID}";

            var r = await _requester.GetSimpleRequest(url, _masterToken.Master_token);

            string json = await r.Content.ReadAsStringAsync();

            ArticleProfitability[] result = JsonConvert.DeserializeObject<ArticleProfitability[]>(json) ?? [];

            return result;
        }
    }
}
