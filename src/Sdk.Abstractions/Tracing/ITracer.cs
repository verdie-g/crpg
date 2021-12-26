namespace Crpg.Sdk.Abstractions.Tracing;

/// <summary>
/// Responsible for manually creating spans and sending to Datadog.
/// </summary>
public interface ITracer
{
    ITraceSpan CreateSpan(string operationName, string? resourceName = null, IEnumerable<KeyValuePair<string, string>>? tags = null);
}
