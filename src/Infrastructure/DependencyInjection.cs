using Crpg.Application.Common.Interfaces.Metrics;
using Crpg.Common;
using Crpg.Infrastructure.Metrics.Datadog;
using Crpg.Infrastructure.Metrics.Debug;
using DatadogStatsD;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crpg.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            AddMetrics(services, configuration, environment);
            services.AddTransient<IDateTime, MachineDateTime>();

            return services;
        }

        private static void AddMetrics(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                services.AddSingleton<IMetricsFactory, DebugMetricsFactory>();
            }
            else
            {
                var dogStatsD = new DogStatsD(new DogStatsDConfiguration
                {
                    Namespace = "crpg",
                    ConstantTags = new[] { "service:crpg_web_api" },
                });
                services.AddSingleton<IMetricsFactory>(new DatadogMetricsFactory(dogStatsD));
            }
        }
    }
}