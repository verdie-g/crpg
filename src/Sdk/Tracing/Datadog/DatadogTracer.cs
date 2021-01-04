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

        public ITraceSpan CreateSpan(string operationName, string? resourceName = null, IEnumerable<KeyValuePair<string, string>>? tags = null)
        {
            Scope scope = Tracer.Instance.StartActive(_namespace + "." + operationName);
            if (resourceName != null)
            {
                scope.Span.ResourceName = resourceName;
            }

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
