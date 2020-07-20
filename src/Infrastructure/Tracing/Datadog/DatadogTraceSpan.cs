using System;
using Crpg.Application.Common.Interfaces.Tracing;
using Datadog.Trace;

namespace Crpg.Infrastructure.Tracing.Datadog
{
    public class DatadogTraceSpan : ITraceSpan
    {
        private readonly Scope _scope;

        public DatadogTraceSpan(Scope scope) => _scope = scope;
        public void SetException(Exception exception) => _scope.Span.SetException(exception);
        public void Dispose() => _scope.Dispose();
    }
}
