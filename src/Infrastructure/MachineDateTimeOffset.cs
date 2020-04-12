using System;
using Crpg.Common;

namespace Crpg.Infrastructure
{
    public class MachineDateTimeOffset : IDateTimeOffset
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}