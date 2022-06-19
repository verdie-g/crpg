using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Crpg.Common.Helpers;

namespace Crpg.Application.Common.Behaviors;

internal class RequestInstrumentation
{
    private static readonly AssemblyName AssemblyName = typeof(RequestInstrumentation).Assembly.GetName();
    private static readonly string InstrumentationName = AssemblyName.Name!;
    private static readonly string InstrumentationVersion = AssemblyName.Version!.ToString();
    private static readonly Meter Meter = new(InstrumentationName, InstrumentationVersion);
    private static readonly ActivitySource ActivitySource = new(InstrumentationName, InstrumentationVersion);

    private readonly ObservableCounter<long> _statusCounter;
    private readonly Histogram<double> _responseTimeHistogram;
    private readonly KeyValuePair<string, object?>[] _responseTimeTags;
    private readonly string _spanName;

    private long _statusOk;
    private long _statusErrorBadRequest;
    private long _statusErrorNotFound;
    private long _statusErrorConflict;
    private long _statusErrorUnknown;

    public RequestInstrumentation(Type requestType)
    {
        const string metricStatus = "crpg.requests.status";
        const string metricResponseTime = "crpg.requests.response-time";
        const string statusKey = "status";

        var requestNameTag = KeyValuePair.Create("name", (object?)StringHelper.PascalToKebabCase(requestType.Name));
        KeyValuePair<string, object?>[] statusOkTags = { requestNameTag, new(statusKey, "ok") };
        KeyValuePair<string, object?>[] statusErrorBadRequestTags = { requestNameTag, new(statusKey, "bad-request") };
        KeyValuePair<string, object?>[] statusErrorNotFoundTags = { requestNameTag, new(statusKey, "not-found") };
        KeyValuePair<string, object?>[] statusErrorConflictTags = { requestNameTag, new(statusKey, "conflict") };
        KeyValuePair<string, object?>[] statusErrorUnknownTags = { requestNameTag, new(statusKey, "unknown") };

        _statusCounter = Meter.CreateObservableCounter(metricStatus, () => new[]
        {
            new Measurement<long>(Volatile.Read(ref _statusOk), statusOkTags),
            new Measurement<long>(Volatile.Read(ref _statusErrorBadRequest), statusErrorBadRequestTags),
            new Measurement<long>(Volatile.Read(ref _statusErrorNotFound), statusErrorNotFoundTags),
            new Measurement<long>(Volatile.Read(ref _statusErrorConflict), statusErrorConflictTags),
            new Measurement<long>(Volatile.Read(ref _statusErrorUnknown), statusErrorUnknownTags),
        });
        _responseTimeHistogram = Meter.CreateHistogram<double>(metricResponseTime);
        _responseTimeTags = new[] { requestNameTag };
        _spanName = StringHelper.PascalToKebabCase(requestType.Name);
    }

    public void IncrementOk() => Interlocked.Increment(ref _statusOk);
    public void IncrementBadRequest() => Interlocked.Increment(ref _statusErrorBadRequest);
    public void IncrementNotFound() => Interlocked.Increment(ref _statusErrorNotFound);
    public void IncrementConflict() => Interlocked.Increment(ref _statusErrorConflict);
    public void IncrementUnknown() => Interlocked.Increment(ref _statusErrorUnknown);
    public void RecordResponseTime(double value) => _responseTimeHistogram.Record(value, _responseTimeTags);
    public Activity? StartRequestSpan()
    {
        return ActivitySource.StartActivity(_spanName);
    }
}
