using System.Collections.Generic;

namespace Crpg.Application.Common.Interfaces.Events
{
    public interface IEventRaiser
    {
        public void Raise(EventLevel eventLevel, string title, string message, string? aggregationKey = null, IList<string>? tags = null);
    }
}