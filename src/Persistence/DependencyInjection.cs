using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                services.AddDbContext<TrpgDbContext>(options => options.UseInMemoryDatabase("trpg"));
            }
            else
            {
                // use pgsql
            }

            services.AddScoped<ITrpgDbContext>(provider => provider.GetService<TrpgDbContext>());

            return services;
        }
    }
}