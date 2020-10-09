using System.Collections.Generic;

namespace Crpg.Sdk.Abstractions.Tracing
{
    public interface ITracer
    {
        ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>> tags);
    }
}
