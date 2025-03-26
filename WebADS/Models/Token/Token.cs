using Newtonsoft.Json;

namespace WebADS.Models.Token
{
    public class Token
    {
        public string access_token { get; set; } = string.Empty;
    }

    public class MasterLogin
    {
        public string master_login { get; set; } = string.Empty;
    }

    public class MasterToken
    {
        private string _filePath;
        public string Master_login { get; set; } = string.Empty;
        public string Master_token { get; set; } = string.Empty;

        public MasterToken(string filePath = "temp.json")
        {
            _filePath = filePath;
            LoadMasterToken();
            LoadMasterLogin();
        }

        public void UpdateMasterLogin(string newLogin)
        {
            Master_login = newLogin;
            MasterLogin masterLogin = new MasterLogin { master_login = newLogin };
            var json = JsonConvert.SerializeObject(masterLogin);
            File.WriteAllText($"login{_filePath}", json);
        }

        public void UpdateMasterToken(string newToken)
        {
            Master_token = newToken;
            Token token = new Token { access_token = newToken };
            var json = JsonConvert.SerializeObject(token, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void LoadMasterToken()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);

                Token token = JsonConvert.DeserializeObject<Token>(json);
                Master_token = token?.access_token ?? string.Empty;
            }
            else
            {
                Master_token = string.Empty;
            }
        }

        private void LoadMasterLogin()
        {
            if (File.Exists($"login{_filePath}"))
            {
                var json = File.ReadAllText($"login{_filePath}");
                MasterLogin masterLogin = JsonConvert.DeserializeObject<MasterLogin>(json);
                Master_login = masterLogin?.master_login ?? string.Empty;
            }
            else
            {
                Master_login = string.Empty;
            }
        }
    }
}
