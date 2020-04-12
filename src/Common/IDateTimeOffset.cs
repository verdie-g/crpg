using System;

namespace Crpg.Common
{
    public interface IDateTimeOffset
    {
        DateTimeOffset Now { get; }
    }
}