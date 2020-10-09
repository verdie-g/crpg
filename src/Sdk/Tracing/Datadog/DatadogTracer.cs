using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Tracing;
using Datadog.Trace;

namespace Crpg.Sdk.Tracing.Datadog
{
    internal class DatadogTracer : ITracer
    {
        private readonly string _namespace;

        public DatadogTracer(string ns)
        {
            _namespace = ns;
        }

        public ITraceSpan CreateSpan(string name, IEnumerable<KeyValuePair<string, string>>? tags = null)
        {
            Scope scope = Tracer.Instance.StartActive(_namespace + "." + name);
            if (tags != null)
            {
                foreach (var kv in tags)
                {
                    scope.Span.SetTag(kv.Key, kv.Value);
                }
            }

            return new DatadogTraceSpan(scope);
        }
    }
}
