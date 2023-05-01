using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Crpg.Logging;

#nullable enable

/// <summary>
/// A static class to create <see cref="Microsoft.Extensions.Logging.ILogger"/>s.
/// </summary>
/// <remarks>This is the only acceptable usage of static to get a component of the SDK.</remarks>
public static class LoggerFactory
{
    /// <summary>
    /// The underlying <see cref="ILoggerFactory"/>. <see cref="Initialize"/> needs to be called before <see cref="CreateLogger{TCategory}"/>.
    /// </summary>
    private static ILoggerFactory _delegate = new NullLoggerFactory();

    /// <summary>
    /// Initialize the global logger with an application configuration.
    /// </summary>
    /// <param name="delegateLoggerFactory">The underlying <see cref="ILoggerFactory"/>.</param>
    public static void Initialize(ILoggerFactory delegateLoggerFactory)
    {
        _delegate = delegateLoggerFactory;
    }

    /// <summary>
    /// Resets and flushes the global logger.
    /// </summary>
    public static void Dispose()
    {
        _delegate.Dispose();
    }

    /// <summary>
    /// Creates a new <see cref="Microsoft.Extensions.Logging.ILogger" /> instance.
    /// </summary>
    /// <typeparam name="TCategory">The category type for messages produced by the logger.</typeparam>
    /// <returns>
    /// The <see cref="Microsoft.Extensions.Logging.ILogger" />.
    /// </returns>
    public static ILogger CreateLogger<TCategory>()
    {
        return CreateLogger(typeof(TCategory));
    }

    /// <summary>
    /// Creates a new <see cref="Microsoft.Extensions.Logging.ILogger" /> instance.
    /// </summary>
    /// <param name="categoryType">The category type for messages produced by the logger.</param>
    /// <returns>
    /// The <see cref="Microsoft.Extensions.Logging.ILogger" />.
    /// </returns>
    public static ILogger CreateLogger(Type categoryType)
    {
        string? categoryName = categoryType.FullName;
        if (categoryName == null)
        {
            throw new InvalidOperationException("Couldn't get full name of type " + categoryType);
        }

        return CreateLogger(categoryName);
    }

    /// <summary>
    /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>
    /// The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.
    /// </returns>
    public static ILogger CreateLogger(string categoryName)
    {
        return _delegate.CreateLogger(categoryName);
    }
}
