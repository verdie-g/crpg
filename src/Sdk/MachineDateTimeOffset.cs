using System;
using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk
{
    public class MachineDateTimeOffset : IDateTimeOffset
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
