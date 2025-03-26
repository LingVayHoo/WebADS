using Microsoft.AspNetCore.Identity;
using WebADS.Data;
using WebADS.Models;

namespace WebADS.Services
{
    public class MoySkladTokenHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyStorageRequester _moySkladRequester;
        private readonly AuthContext _authContext;

        public MoySkladTokenHandler(
            UserManager<ApplicationUser> userManager,
            MyStorageRequester moySkladRequester,
            AuthContext authContext)
        {
            _userManager = userManager;
            _moySkladRequester = moySkladRequester;
            _authContext = authContext;
        }

        public async Task<string> GetMoySkladTokenFromDB(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            var token = user?.MoySkladToken ?? string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                token = await UpdateMoySkladToken(username);
            }

            return token;
        }

        public async Task<string> UpdateMoySkladToken(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            var msUsername = user?.MoySkladUsername ?? string.Empty;
            var msPassword = user?.MoySkladPassword ?? string.Empty;


            if (string.IsNullOrEmpty(user?.MoySkladUsername) || string.IsNullOrEmpty(user.MoySkladPassword))
            {
                return string.Empty;
            }

            try
            {
                var token = await _moySkladRequester.FetchTokenAsync(msUsername, msPassword);
                await UpdateTokenEverywhere(msUsername, token);
                return token;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> ConnectAccounts(
            string username,
            string MoySkladUsername,
            string MoySkladPassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (string.IsNullOrEmpty(MoySkladUsername) ||
                string.IsNullOrEmpty(MoySkladPassword) ||
                user == null)
            {
                return string.Empty;
            }

            try
            {
                var token = await _moySkladRequester.FetchTokenAsync(MoySkladUsername, MoySkladPassword);
                if (!string.IsNullOrEmpty(token))
                {
                    user.MoySkladUsername = MoySkladUsername;
                    user.MoySkladPassword = MoySkladPassword;
                    await _userManager.UpdateAsync(user);
                    await UpdateTokenEverywhere(MoySkladUsername, token);
                    return token;
                }
                return string.Empty;

            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        private async Task UpdateTokenEverywhere(string moySkladUsername, string moySkladToken)
        {
            var users = _authContext.Users;
            var filtered = users.Where(u => u.MoySkladUsername == moySkladUsername);

            foreach (var user in filtered)
            {
                user.MoySkladToken = moySkladToken;
            }
            await _authContext.SaveChangesAsync();
        }
    }
}
