using System.Collections.Generic;

namespace Crpg.Sdk.Abstractions.Events
{
    public interface IEventRaiser
    {
        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<string>? tags = null);
    }
}
