using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crpg.Persistence
{
    public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseNpgsql("Database=crpg")
                .UseSnakeCaseNamingConvention()
                .Options;
            return CreateNewInstance(options);
        }

        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);
    }
}