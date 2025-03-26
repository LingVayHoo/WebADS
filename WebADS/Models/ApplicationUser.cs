using Microsoft.AspNetCore.Identity;

namespace WebADS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool MustChangePassword { get; set; } = true;
        public string? Name { get; set; }
        public string? MoySkladUsername { get; set; }
        public string? MoySkladPassword { get; set; }
        public string? MoySkladToken { get; set; }
    }
}
