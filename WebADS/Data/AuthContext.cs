using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebADS.Models;

namespace WebADS.Data
{
    public class AuthContext : IdentityDbContext<ApplicationUser>
    {
        public AuthContext(DbContextOptions<AuthContext> options)
            : base(options)
        {

        }
    }
}
