using Microsoft.Extensions.Hosting;
using Serilog;

namespace Crpg.Logging;

/// <summary>
/// Extends <see cref="Microsoft.Extensions.Hosting.IHostBuilder" /> with Crpg.Logging configuration methods.
/// </summary>
public static class LoggingHostBuilderExtension
{
    /// <summary>Sets Crpg.Logging as the logging provider.</summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder UseLogging(this IHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.UseSerilog();
        return builder;
    }
}
