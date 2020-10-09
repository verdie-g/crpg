using System;
using Crpg.Common;

namespace Crpg.Sdk
{
    public class MachineDateTimeOffset : IDateTimeOffset
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
