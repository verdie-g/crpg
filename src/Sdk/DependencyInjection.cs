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

namespace Crpg.Sdk;

public static class DependencyInjection
{
    public static IServiceCollection AddSdk(this IServiceCollection services,
        IConfiguration configuration, IApplicationEnvironment appEnv)
    {
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
            DogStatsD dogStatsD = new(new DogStatsDConfiguration
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

    private static KeyValuePair<string, string>[] BuildTagsFromEnv(IApplicationEnvironment appEnv)
    {
        List<KeyValuePair<string, string>> constantTags = new()
        {
            new("service", appEnv.ServiceName),
        };

        if (appEnv.Instance.Length != 0)
        {
            constantTags.Add(new KeyValuePair<string, string>("instance", appEnv.Instance));
        }

        return constantTags.ToArray();
    }
}
