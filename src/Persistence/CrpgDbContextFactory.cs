using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crpg.Persistence;

/// <summary>
/// Provides options for "dotnet ef" tool.
/// </summary>
public class CrpgDbContextFactory : IDesignTimeDbContextFactory<CrpgDbContext>
{
    public CrpgDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CrpgDbContext>()
            .UseNpgsql("Host=localhost;Database=crpg;Username=postgres;Password=root", options => options.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;
        return new CrpgDbContext(options);
    }
}
