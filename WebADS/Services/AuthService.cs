using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using WebADS.Models.MoySklad.Storages;
using WebADS.Models.Token;

namespace WebADS.Services
{
    public class AuthService
    {
        private MyStorageRequester _myStorageRequester;
        private MasterToken _masterToken;

        public AuthService(MyStorageRequester myStorageRequester, MasterToken masterToken)
        {
            _myStorageRequester = myStorageRequester;
            _masterToken = masterToken;
        }

        public async Task<Dictionary<string, string>> GetStorages(string moySkladUsername, string token = "default")
        {
            //if (token == "default")
            //{
            //    token = _masterToken.Master_token;
            //}
            string accountID = await GetMoySkladUserID(moySkladUsername);

            if (!string.IsNullOrEmpty(accountID))
            {
                //var result = await FetchStoragesByAccountIdAsync(accountID, _masterToken.Master_token);
                var result = await FetchStoragesByAccountIdAsync(accountID, token);
                if (result != null)
                {
                    return result;
                }
            }           

            return [];
        }

        // Метод для получения токена с использованием учетных данных
        private async Task<bool> CheckCredentials(string username, string password)
        {
            string url = "https://api.moysklad.ru/api/remap/1.2/security/token";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

                HttpResponseMessage response = await client.PostAsync(url, null);

                return response.IsSuccessStatusCode;
            }            
        }

        private async Task<string> GetMoySkladUserID(string username)
        {
            string url = $"https://api.moysklad.ru/api/remap/1.2/entity/employee?filter=uid={username}";

            string json = await _myStorageRequester.GetRequestAsync(url, _masterToken.Master_token);

            JObject data = JObject.Parse(json);

            if (data["rows"] is JArray rows)
            {
                var ids = rows
                    .Select(row => row["id"]?.ToString()) // Проверяем на null
                    .Where(id => !string.IsNullOrEmpty(id)).ToArray(); // Исключаем пустые значения

                if (ids.Length > 0)
                {
                    return ids[0];
                }                
            }
            return string.Empty;
        }

        private async Task<Dictionary<string, string>> FetchStoragesByAccountIdAsync(string accountId, string userToken)
        {
            //string url = $"https://api.moysklad.ru/api/remap/1.2/entity/store?filter=accountId={accountId}";
            string url = $"https://api.moysklad.ru/api/remap/1.2/entity/store";
            string responseBody = await _myStorageRequester.GetRequestAsync(url, userToken);
            RootObject root = JsonConvert.DeserializeObject<RootObject>(responseBody);

            Dictionary<string, string> storages = new Dictionary<string, string>();

            if (root?.Rows != null)
            {
                foreach (var row in root.Rows)
                {
                    storages.Add(row.Id, row.Name);
                }
            }

            return storages;
        }
    }
}
