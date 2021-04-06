using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpgradeOctoSystem.Database.Models;

namespace UpgradeOctoSystem.Database
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
          {
              builder
              .AddConsole((options) => { })
              .AddFilter((category, level) =>
                  category == DbLoggerCategory.Database.Command.Name
                  && level == LogLevel.Information);
          });

        public DatabaseContext()
        {
            // Constructor used by ef tools
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            // Constructor used by DI to create instance
        }

        public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer("Data Source=.;Initial Catalog=UpgradedOcto;Integrated Security=True") // TODO: abstract to config
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
        }
    }
}