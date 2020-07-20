using System.Collections.Generic;

namespace Crpg.Application.Common.Interfaces.Tracing
{
    public interface ITracer
    {
        ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>> tags);
    }
}
