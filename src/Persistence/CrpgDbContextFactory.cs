using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crpg.Persistence
{
    public class CrpgDbContextFactory : IDesignTimeDbContextFactory<CrpgDbContext>
    {
        public CrpgDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .UseNpgsql("Database=crpg", options => options.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;
            return new CrpgDbContext(options);
        }
    }
}
