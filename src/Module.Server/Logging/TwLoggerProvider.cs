using Microsoft.Extensions.Logging;

namespace Crpg.Module.Logging;

[ProviderAlias("TW")]
public class TwLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => TwLogger.Instance;

    public void Dispose()
    {
    }
}
