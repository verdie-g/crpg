using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crpg.Persistence
{
    public class CrpgDbContextFactory : IDesignTimeDbContextFactory<CrpgDbContext>
    {
        public CrpgDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .UseNpgsql("Database=crpg")
                .UseSnakeCaseNamingConvention()
                .Options;
            return new CrpgDbContext(options);
        }
    }
}