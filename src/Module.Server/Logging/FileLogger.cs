using Microsoft.Extensions.Logging;

namespace Crpg.Module.Logging;

public class FileLogger : ILogger
{
    private readonly Dictionary<LogLevel, string> _logLevelToTxt = new()
    {
        [LogLevel.Trace] = "TRC",
        [LogLevel.Debug] = "DBG",
        [LogLevel.Information] = "INF",
        [LogLevel.Warning] = "WRN",
        [LogLevel.Error] = "ERR",
        [LogLevel.Critical] = "CRT",
        [LogLevel.None] = "NOP",
    };

    private readonly string _name;
    private readonly StreamWriter _writer;

    public FileLogger(string name, StreamWriter writer)
    {
        _name = name;
        _writer = writer;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string message = formatter(state, exception)
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
        string log = $"{DateTime.UtcNow:o} {_logLevelToTxt[logLevel]} {_name} {message}";
        lock (_writer)
        {
            _writer.WriteLine(log);
        }
    }
}
