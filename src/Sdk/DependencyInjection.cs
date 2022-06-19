using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Tracing.Exporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Crpg.Sdk;

public static class DependencyInjection
{
    public static IServiceCollection AddSdk(this IServiceCollection services,
        IConfiguration configuration, IApplicationEnvironment appEnv)
    {
        return services
            .AddOpenTelemetryMetrics(opts => ConfigureOpenTelemetryMetrics(opts, appEnv))
            .AddOpenTelemetryTracing(opts => ConfigureOpenTelemetryTracing(opts, appEnv))
            .AddSingleton(appEnv)
            .AddSingleton<IDateTime, MachineDateTime>()
            .AddSingleton<IRandom, ThreadSafeRandom>();
    }

    private static void ConfigureOpenTelemetryMetrics(MeterProviderBuilder meterProviderBuilder, IApplicationEnvironment appEnv)
    {
        meterProviderBuilder
            .SetResourceBuilder(CreateResourceBuilder(appEnv));

        if (appEnv.Environment != HostingEnvironment.Development)
        {
            meterProviderBuilder.AddOtlpExporter(opts => opts.Protocol = OtlpExportProtocol.Grpc);
        }
    }

    private static void ConfigureOpenTelemetryTracing(TracerProviderBuilder tracerProviderBuilder,
        IApplicationEnvironment appEnv)
    {
        tracerProviderBuilder
            .SetResourceBuilder(CreateResourceBuilder(appEnv))
            .AddSource("*")
            .SetErrorStatusOnException();

        if (appEnv.Environment == HostingEnvironment.Development)
        {
            tracerProviderBuilder.AddProcessor(new SimpleActivityExportProcessor(new LoggingExporter()));
        }
        else
        {
            tracerProviderBuilder.AddOtlpExporter(opts =>
            {
                opts.Protocol = OtlpExportProtocol.Grpc;
            });
        }
    }

    private static ResourceBuilder CreateResourceBuilder(IApplicationEnvironment appEnv)
    {
        return ResourceBuilder.CreateEmpty()
            .AddService(
                serviceName: appEnv.ServiceName,
                serviceVersion: typeof(DependencyInjection).Assembly.GetName().Version!.ToString(),
                serviceInstanceId: appEnv.Instance);
    }
}
