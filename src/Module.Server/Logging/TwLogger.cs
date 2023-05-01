using Microsoft.Extensions.Logging;
using TaleWorlds.Library;

namespace Crpg.Module.Logging;

public class TwLogger : ILogger
{
    public static ILogger Instance { get; } = new TwLogger();

    private readonly Dictionary<LogLevel, Debug.DebugColor> _logLevelToColor = new()
    {
        [LogLevel.Trace] = Debug.DebugColor.White,
        [LogLevel.Debug] = Debug.DebugColor.White,
        [LogLevel.Information] = Debug.DebugColor.White,
        [LogLevel.Warning] = Debug.DebugColor.Yellow,
        [LogLevel.Error] = Debug.DebugColor.Red,
        [LogLevel.Critical] = Debug.DebugColor.DarkRed,
        [LogLevel.None] = Debug.DebugColor.White,
    };

    private TwLogger()
    {
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

        Debug.Print(
            message: formatter(state, exception),
            logLevel: (int)logLevel,
            color: _logLevelToColor[logLevel]);
    }
}
