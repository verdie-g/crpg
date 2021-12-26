namespace Crpg.Sdk.Abstractions.Events;

/// <summary>
/// Responsible for sending events to Datadog.
/// </summary>
public interface IEventService
{
    public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<KeyValuePair<string, string>>? tags = null);
}
