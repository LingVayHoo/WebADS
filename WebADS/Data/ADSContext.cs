using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebADS.Models;
using WebADS.Models.Token;

namespace WebADS.Data
{
    public class ADSContext : DbContext
    {
        public DbSet<AddressDBModel> addresses { get; set; } = null!;
        public DbSet<SAC> sacs { get; set; } = null!;
        public DbSet<UserToken> usertokens { get; set; } = null!;
        public DbSet<AddressHistoryDBModel> addresseshistory { get; set; } = null!;
        public DbSet<StorageSettings> storagesettings { get; set; } = null!;
        public DbSet<ArticleParameters> articleparameters { get; set; } = null!;

        public ADSContext(DbContextOptions<ADSContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AddressDBModel>()
                .HasIndex(a => a.Article)
                .HasFilter("\"IsPrimaryPlace\" = TRUE")
                .IsUnique();

            modelBuilder.Entity<ArticleParameters>()
                .HasIndex(a => a.ProductID)
                .IsUnique();
        }
    }
}


