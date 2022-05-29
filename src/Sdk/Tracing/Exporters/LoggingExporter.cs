using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Sdk.Tracing.Exporters;

internal class LoggingExporter : BaseExporter<Activity>
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<LoggingExporter>();

    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (var activity in batch)
        {
            var sb = new StringBuilder($"Span {activity.DisplayName}{{");
            bool hasTags = false;
            foreach (var tag in activity.Tags)
            {
                sb.Append($"{tag.Key}={tag.Value}, ");
                hasTags = true;
            }

            if (hasTags)
            {
                sb.Length -= ", ".Length;
            }

            sb.Append("} ended");
            Logger.Log(LogLevel.Debug, sb.ToString());
        }

        return ExportResult.Success;
    }
}
