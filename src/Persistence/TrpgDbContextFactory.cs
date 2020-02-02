using Microsoft.EntityFrameworkCore;

namespace Trpg.Persistence
{
    public class TrpgDbContextFactory : DesignTimeDbContextFactoryBase<TrpgDbContext>
    {
        protected override TrpgDbContext CreateNewInstance(DbContextOptions<TrpgDbContext> options)
        {
            return new TrpgDbContext(options);
        }
    }
}