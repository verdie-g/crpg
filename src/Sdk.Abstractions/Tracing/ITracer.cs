using System.Collections.Generic;

namespace Crpg.Sdk.Abstractions.Tracing
{
    /// <summary>
    /// Responsible for manually creating spans and sending to Datadog.
    /// </summary>
    public interface ITracer
    {
        ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>> tags);
    }
}
