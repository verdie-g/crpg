using System;

namespace Crpg.Sdk.Abstractions.Tracing
{
    public interface ITraceSpan : IDisposable
    {
        void SetException(Exception exception);
    }
}
