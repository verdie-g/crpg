using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crpg.Sdk.Abstractions.Tracing;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Sdk.Tracing.Debug
{
    internal class DebugTraceSpan : ITraceSpan
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DebugTraceSpan>();

        private readonly string _operationName;
        private readonly string? _resourceName;
        private readonly IEnumerable<KeyValuePair<string, string>>? _tags;

        public DebugTraceSpan(string operationName, string? resourceName, IEnumerable<KeyValuePair<string, string>>? tags)
        {
            _operationName = operationName;
            _resourceName = resourceName;
            _tags = tags;
        }

        public void SetException(Exception exception)
        {
        }

        public void Dispose() => Logger.Log(LogLevel.Debug, "End of trace {0}", ToString());

        public override string ToString()
        {
            var sb = new StringBuilder(_operationName);
            if (_resourceName != null)
            {
                sb.Append(' ');
                sb.Append(_resourceName);
            }

            if (_tags != null && _tags.Any())
            {
                sb.Append(' ');
                sb.Append('[');
                foreach (var kv in _tags)
                {
                    sb.AppendFormat("{0}:{1}, ", kv.Key, kv.Value);
                }

                if (sb.Length != 0)
                {
                    sb.Length -= 2;
                }

                sb.Append(']');
            }

            return sb.ToString();
        }
    }
}
