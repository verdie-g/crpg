using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Crpg.Common;

namespace Crpg.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddTransient<IDateTime, MachineDateTime>();

            return services;
        }
    }
}