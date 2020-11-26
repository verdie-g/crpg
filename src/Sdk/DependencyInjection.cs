using System;
using System.Collections.Generic;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Events;
using Crpg.Sdk.Abstractions.Metrics;
using Crpg.Sdk.Abstractions.Tracing;
using Crpg.Sdk.Events;
using Crpg.Sdk.Metrics.Datadog;
using Crpg.Sdk.Metrics.Debug;
using Crpg.Sdk.Tracing.Datadog;
using Crpg.Sdk.Tracing.Debug;
using DatadogStatsD;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crpg.Sdk
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSdk(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            var appEnv = CreateApplicationEnvironment(environment);
            return services
                .AddDatadog(appEnv)
                .AddSingleton(appEnv)
                .AddSingleton<IDateTimeOffset, MachineDateTimeOffset>()
                .AddSingleton<IRandom, ThreadSafeRandom>();
        }

        private static IServiceCollection AddDatadog(this IServiceCollection services, IApplicationEnvironment appEnv)
        {
            if (appEnv.Environment == HostingEnvironment.Development)
            {
                services.AddSingleton<IMetricsFactory, DebugMetricsFactory>();
                services.AddSingleton<IEventService, DebugEventService>();
                services.AddSingleton<ITracer, DebugTracer>();
            }
            else
            {
                var dogStatsD = new DogStatsD(new DogStatsDConfiguration
                {
                    Namespace = "crpg",
                    ConstantTags = BuildTagsFromEnv(appEnv),
                });

                services.AddSingleton<IMetricsFactory>(new DatadogMetricsFactory(dogStatsD));
                services.AddSingleton<IEventService>(new DatadogEventService(dogStatsD));
                services.AddSingleton<ITracer>(new DatadogTracer("crpg"));
            }

            return services;
        }

        private static IApplicationEnvironment CreateApplicationEnvironment(IHostEnvironment environment)
        {
            var env = environment.IsProduction() ? HostingEnvironment.Production : HostingEnvironment.Development;
            string serviceName = Environment.GetEnvironmentVariable("CRPG_SERVICE") ?? "test";
            string instance = Environment.GetEnvironmentVariable("CRPG_INSTANCE") ?? string.Empty;
            return new ApplicationEnvironment(env, serviceName, instance, Environment.MachineName);
        }

        private static KeyValuePair<string, string>[] BuildTagsFromEnv(IApplicationEnvironment appEnv)
        {
            var constantTags = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("service", appEnv.ServiceName),
            };

            if (appEnv.Instance.Length != 0)
            {
                constantTags.Add(new KeyValuePair<string, string>("instance", appEnv.Instance));
            }

            return constantTags.ToArray();
        }
    }
}
