using System;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Common.Interfaces.Metrics;
using Crpg.Common;
using Crpg.Infrastructure.Events;
using Crpg.Infrastructure.Files;
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
            var appEnv = CreateApplicationEnvironment(environment);
            return services
                .AddDatadog(appEnv)
                .AddSingleton(appEnv)
                .AddSingleton<IDateTimeOffset, MachineDateTimeOffset>()
                .AddSingleton<IRandom, ThreadSafeRandom>()
                .AddSingleton<IItemsSource, FileItemsSource>();
        }

        private static IServiceCollection AddDatadog(this IServiceCollection services, IApplicationEnvironment appEnv)
        {
            if (appEnv.Environment == HostingEnvironment.Development)
            {
                services.AddSingleton<IMetricsFactory, DebugMetricsFactory>();
                services.AddSingleton<IEventRaiser, DebugEventRaiser>();
            }
            else
            {
                var dogStatsD = new DogStatsD(new DogStatsDConfiguration
                {
                    Namespace = "crpg",
                    ConstantTags = new[] { "service:" + appEnv.ServiceName },
                });

                services.AddSingleton<IMetricsFactory>(new DatadogMetricsFactory(dogStatsD));
                services.AddSingleton<IEventRaiser>(new DatadogEventRaiser(dogStatsD));
            }

            return services;
        }

        private static IApplicationEnvironment CreateApplicationEnvironment(IHostEnvironment environment)
        {
            var env = environment.IsProduction() ? HostingEnvironment.Production : HostingEnvironment.Development;
            return new ApplicationEnvironment(env, "crpg_web_api", Environment.MachineName);
        }
    }
}
