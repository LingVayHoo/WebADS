using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Drawing;
using WebADS.Models;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSPlaceSearchModel : PageModel
    {
        private readonly ADSReports _aDSReports;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthService _authService;

        [BindProperty]
        public string SelectedStore { get; set; } = "-- Выберите значение --";
        public Dictionary<string, string> Stores { get; set; } = new();

        [BindProperty]
        public string SelectedZone { get; set; } = "all";
        public List<string> Zones { get; set; } = new();

        [BindProperty]
        public string SelectedRow { get; set; } = "all";
        public List<string> Rows { get; set; } = new();

        [BindProperty]
        public string SelectedPlace { get; set; } = "all";
        public List<string> Places { get; set; } = new();

        [BindProperty]
        public string SelectedLevel { get; set; } = "all";
        public List<string> Levels { get; set; } = new();

        public ADSPlaceSearchModel(
            ADSReports aDSReports,
            UserManager<ApplicationUser> userManager,
            AuthService authService)
        {
            _aDSReports = aDSReports;
            _userManager = userManager;
            _authService = authService;
        }

        public async Task OnGet()
        {
            await UpdateStoresData();
        }

        public async Task<JsonResult> OnPostGetAdressesByPlaceAsync(
            string selectedStore,
            string selectedZone,
            string selectedRow,
            string selectedPlace,
            string selectedLevel)
        {
            await UpdateStoresData();

            if (!Stores.ContainsKey(selectedStore))
            {
                return new JsonResult(new List<string> { "all" }); // Если нет в списке, возвращаем "all"
            }

            SelectedStore = selectedStore; // Запоминаем выбранный склад

            var neededStore = Stores[selectedStore];

            if (string.IsNullOrEmpty(selectedZone) || 
                selectedZone == "NoData" || 
                selectedZone == "-- Выберите значение --")
            {
                return new JsonResult(new List<string>());
            }

            if (string.IsNullOrEmpty(
                selectedRow) ||
                selectedRow == "NoData" ||
                selectedRow == "-- Выберите значение --")
            {
                return new JsonResult(new List<string>());
            }

            if (string.IsNullOrEmpty(
                selectedPlace) ||
                selectedPlace == "NoData" ||
                selectedPlace == "-- Выберите значение --")
            {
                return new JsonResult(new List<string>());
            }

            if (string.IsNullOrEmpty(
                selectedLevel) ||
                selectedLevel == "NoData" ||
                selectedLevel == "-- Выберите значение --")
            {
                return new JsonResult(new List<string>());
            }


            List<AddressDBModel> result = new List<AddressDBModel>();

            result = await _aDSReports.GetAddressesByPlace(
                neededStore, 
                selectedZone,
                selectedRow,
                selectedPlace,
                selectedLevel);

            return new JsonResult(result);
        }
                
        public async Task<JsonResult> OnPostGetValuesAsync(
            string selectedStore,
            string selectedZone,
            string selectedRow,
            string selectedPlace)
        {
            await UpdateStoresData();

            if (!Stores.ContainsKey(selectedStore))
            {
                return new JsonResult(new List<string> { "all" }); // Если нет в списке, возвращаем "all"
            }

            SelectedStore = selectedStore; // Запоминаем выбранный склад

            var neededStore = Stores[selectedStore];     
            
            List<string> filters = new List<string>();

            if (selectedZone != "NoData")
            {
                filters.Add(selectedZone);
                SelectedZone = selectedZone;
            }

            if (selectedRow != "NoData")
            {
                filters.Add(selectedRow);
                SelectedRow = selectedRow;
            }

            if (selectedPlace != "NoData")
            {
                filters.Add(selectedPlace);
                SelectedPlace = selectedPlace;
            }


            List<string> result = new List<string>();

            if (filters.Count == 0)
            {
                result = await _aDSReports.GetUniqueValuesAsync(neededStore);
            }
            else
            {
                result = await _aDSReports.GetUniqueValuesAsync(neededStore, filters.ToArray());
            }

            result.Add("all");

            return new JsonResult(result);
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
