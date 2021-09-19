using System.Collections.Generic;
using Crpg.Sdk.Abstractions.Tracing;
using Datadog.Trace;

namespace Crpg.Sdk.Tracing.Datadog
{
    internal class DatadogTracer : ITracer
    {
        private static readonly KeyValuePair<string, string>[] DefaultTags = { new("component", "crpg") };

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

            foreach (var tag in DefaultTags)
            {
                scope.Span.SetTag(tag.Key, tag.Value);
            }

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    scope.Span.SetTag(tag.Key, tag.Value);
                }
            }

            return new DatadogTraceSpan(scope);
        }
    }
}
