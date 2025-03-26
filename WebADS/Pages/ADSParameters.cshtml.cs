using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using WebADS.Models;
using WebADS.Services;

namespace WebADS.Pages
{
    [Authorize]
    public class ADSParametersModel : PageModel
    {
        private readonly MyStorageAPIModel _myStorageAPIModel;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ArticleParametersHandler _articleParametersHandler;


        public ADSParametersModel(
            MyStorageAPIModel myStorageAPIModel,
            UserManager<ApplicationUser> userManager,
            ArticleParametersHandler articleParametersHandler)
        {
            _myStorageAPIModel = myStorageAPIModel;
            _userManager = userManager;
            _articleParametersHandler = articleParametersHandler;
        }

        [BindProperty]
        public string Search { get; set; } = string.Empty;
        [BindProperty]
        public List<ProductModel> SearchResults { get; set; } = new();

        public void OnGet()
        {

        }

        public async Task<JsonResult> OnPostUpdateAWSAsync()
        {
            string m = await _articleParametersHandler.UpdateAWS();
            return new JsonResult(m);
        }

    }
}
