using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Crpg.Logging
{
    /// <summary>
    /// A static class to create <see cref="Microsoft.Extensions.Logging.ILogger"/>s.
    /// </summary>
    /// <remarks>This is the only acceptable usage of static to get a component of the SDK.</remarks>
    public static class LoggerFactory
    {
        /// <summary>
        /// A factory for serilog loggers. It uses <see cref="Log.Logger"/> so <see cref="Initialize"/> needs to be called
        /// before <see cref="CreateLogger{TCategory}"/>.
        /// </summary>
        private static readonly SerilogLoggerFactory UnderlyingLoggerFactory = new();

        /// <summary>
        /// Initialize the global logger with an application configuration.
        /// </summary>
        /// <param name="configuration">The application configuration. The "Serilog" section is used.</param>
        public static void Initialize(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        /// <summary>
        /// Resets and flushes the global logger.
        /// </summary>
        public static void Close()
        {
            Log.CloseAndFlush();
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

            return UnderlyingLoggerFactory.CreateLogger(categoryName);
        }
    }
}
