using System;

namespace Crpg.Application.Common.Interfaces.Tracing
{
    public interface ITraceSpan : IDisposable
    {
        void SetException(Exception exception);
    }
}
