using Microsoft.EntityFrameworkCore;

namespace Crpg.Persistence
{
    public class CrpgDbContextFactory : DesignTimeDbContextFactoryBase<CrpgDbContext>
    {
        protected override CrpgDbContext CreateNewInstance(DbContextOptions<CrpgDbContext> options)
        {
            return new CrpgDbContext(options);
        }
    }
}