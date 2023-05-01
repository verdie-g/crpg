using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Crpg.Module.Logging;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string path)
    {
        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, n => new FileLogger(n, _writer));

    public void Dispose()
    {
        _writer.Dispose();
    }
}
