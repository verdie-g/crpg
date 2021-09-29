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
            string connectionString = configuration.GetConnectionString("Crpg");
            if (appEnv.Environment == HostingEnvironment.Development && connectionString == null)
            {
                services.AddDbContext<CrpgDbContext>(options => options.UseInMemoryDatabase("crpg"));
            }
            else
            {
                services.AddDbContext<CrpgDbContext>(options =>
                {
                    options
                        .UseNpgsql(connectionString, options => options.UseNetTopologySuite())
                        .UseSnakeCaseNamingConvention();
                    if (appEnv.Environment == HostingEnvironment.Development)
                    {
                        options
                            .EnableSensitiveDataLogging()
                            .EnableDetailedErrors();
                    }
                });
            }

            services.AddScoped<ICrpgDbContext>(provider => provider.GetRequiredService<CrpgDbContext>());

            return services;
        }
    }
}
