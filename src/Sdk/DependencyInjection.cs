using System;
using Crpg.Application.Common.Interfaces;
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
                services.AddSingleton<IEventRaiser, DebugEventRaiser>();
                services.AddSingleton<ITracer, DebugTracer>();
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
                services.AddSingleton<ITracer>(new DatadogTracer("crpg"));
            }

            return services;
        }

        private static IApplicationEnvironment CreateApplicationEnvironment(IHostEnvironment environment)
        {
            var env = environment.IsProduction() ? HostingEnvironment.Production : HostingEnvironment.Development;
            string serviceName = Environment.GetEnvironmentVariable("CRPG_SERVICE") ?? "test";
            return new ApplicationEnvironment(env, serviceName, Environment.MachineName);
        }
    }
}
