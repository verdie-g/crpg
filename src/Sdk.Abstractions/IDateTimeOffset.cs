using System;

namespace Crpg.Sdk.Abstractions
{
    /// <summary>
    /// Abstracts the system clock to facilitate testing.
    /// </summary>
    public interface IDateTimeOffset
    {
        /// <summary>
        /// Retrieves the current system time with an offset set to the local time's offset from Coordinated Universal Time (UTC).
        /// </summary>
        DateTimeOffset Now { get; }
    }
}
