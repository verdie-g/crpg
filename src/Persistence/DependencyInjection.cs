using Crpg.Application.Common.Interfaces;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services,
            IConfiguration configuration, IApplicationEnvironment appEnv)
        {
            if (appEnv.Environment == HostingEnvironment.Development)
            {
                services.AddDbContext<CrpgDbContext>(options => options.UseInMemoryDatabase("crpg"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("Crpg");
                services.AddDbContext<CrpgDbContext>(options =>
                    options
                        .UseNpgsql(connectionString, options => options.UseNetTopologySuite())
                        .UseSnakeCaseNamingConvention());
            }

            services.AddScoped<ICrpgDbContext>(provider => provider.GetRequiredService<CrpgDbContext>());

            return services;
        }
    }
}
