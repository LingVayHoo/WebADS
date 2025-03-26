using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebADS.Models;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSReportsPageModel : PageModel
    {
        private readonly ADSReports _aDSReports;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthService _authService;

        public Dictionary<string, string> Stores { get; set; } = new();
        public List<string> Zones { get; set; } = new();

        [BindProperty]
        public string SelectedStore { get; set; } = "all";

        [BindProperty]
        public string SelectedZone { get; set; } = "all";

        public ADSReportsPageModel(ADSReports aDSReports, UserManager<ApplicationUser> userManager, AuthService authService)
        {
            _aDSReports = aDSReports;
            _userManager = userManager;
            _authService = authService;
        }

        public async Task OnGet()
        {
            await UpdateStoresData();
        }

        public async Task<JsonResult> OnPostGetZonesByStoreIDAsync(string selectedStore)
        {
            await UpdateStoresData();

            if (!Stores.ContainsKey(selectedStore))
            {
                return new JsonResult(new List<string> { "all" }); // Если нет в списке, возвращаем "all"
            }

            SelectedStore = selectedStore; // Запоминаем выбранный склад

            var neededStore = Stores[selectedStore];

            var zones = await _aDSReports.GetUniqueValuesAsync(neededStore);
            zones.Add("all");

            return new JsonResult(zones);
        }


        public async Task<FileResult> OnPostGetExcelReportAsync(string selectedStore, string selectedZone)
        {

            await UpdateStoresData();

            if (!Stores.ContainsKey(selectedStore))
            {
                selectedStore = "all"; // Используем "all" как запасной вариант
            }
            SelectedStore = selectedStore; // Запоминаем выбранный склад

            var neededStore = Stores[selectedStore];

            var report = await _aDSReports.GetADSReportExcelAsync(neededStore, selectedZone);

            return report;
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

                // Сохраняем в Session
                HttpContext.Session.SetString("Stores", JsonConvert.SerializeObject(Stores));
            }
        }
    }
}
