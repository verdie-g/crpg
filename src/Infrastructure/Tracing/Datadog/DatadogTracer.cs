using System.Collections.Generic;
using Crpg.Application.Common.Interfaces.Tracing;
using Datadog.Trace;

namespace Crpg.Infrastructure.Tracing.Datadog
{
    internal class DatadogTracer : ITracer
    {
        public ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>>? tags = null)
        {
            Scope scope = Tracer.Instance.StartActive(name);
            if (tags != null)
            {
                foreach (var (key, value) in tags)
                {
                    scope.Span.SetTag(key, value);
                }
            }

            return new DatadogTraceSpan(scope);
        }
    }
}
