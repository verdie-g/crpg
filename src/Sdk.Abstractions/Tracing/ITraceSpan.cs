using System;

namespace Crpg.Sdk.Abstractions.Tracing
{
    public interface ITraceSpan : IDisposable
    {
        /// <summary>
        /// Add an exception to the span.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void SetException(Exception exception);
    }
}
