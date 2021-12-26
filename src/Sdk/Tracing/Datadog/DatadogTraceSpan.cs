using Crpg.Sdk.Abstractions.Tracing;
using Datadog.Trace;

namespace Crpg.Sdk.Tracing.Datadog;

public class DatadogTraceSpan : ITraceSpan
{
    private readonly IScope _scope;

    public DatadogTraceSpan(IScope scope) => _scope = scope;
    public void SetException(Exception exception) => _scope.Span.SetException(exception);
    public void Dispose() => _scope.Dispose();
}
