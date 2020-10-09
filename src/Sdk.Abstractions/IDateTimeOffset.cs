using System;

namespace Crpg.Sdk.Abstractions
{
    public interface IDateTimeOffset
    {
        DateTimeOffset Now { get; }
    }
}
