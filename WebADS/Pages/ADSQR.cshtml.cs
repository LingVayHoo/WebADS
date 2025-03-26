using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebADS.Models;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSQRModel : PageModel
    {
        private readonly ADSReports _aDSReports;

        public ADSQRModel(ADSReports aDSReports)
        {
            _aDSReports = aDSReports;
        }

        public List<AddressDBModel> Addresses { get; set; }
        public string Store { get; set; }
        public string Zone { get; set; }
        public string Row { get; set; }
        public string Place { get; set; }
        public string Level { get; set; }

        public async Task OnGetAsync(string store, string zone, string row, string place, string level)
        {
            // Сохраняем параметры в модель для отображения
            Store = store;
            Zone = zone;
            Row = row;
            Place = place;
            Level = level;

            // Вызываем сервис для получения данных
            Addresses = await _aDSReports.GetAddressesByPlace(store, zone, row, place, level);
        }
    }

}
