using Crpg.Application.Common.Interfaces;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services,
        IConfiguration configuration,
        IApplicationEnvironment appEnv,
        Action<DbContextOptionsBuilder>? dbOptionsAction = null)
    {
        string? connectionString = configuration.GetConnectionString("Crpg");
        if (appEnv.Environment == HostingEnvironment.Development && connectionString == null)
        {
            services.AddDbContext<CrpgDbContext>(options =>
            {
                options.UseInMemoryDatabase("crpg");
                dbOptionsAction?.Invoke(options);
            });
        }
        else
        {
            services.AddDbContext<CrpgDbContext>(options =>
            {
                options
                    .UseNpgsql(connectionString, options => options.UseNetTopologySuite())
                    .UseSnakeCaseNamingConvention();
                dbOptionsAction?.Invoke(options);
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
