using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.System.Commands;
using Crpg.Persistence;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;

namespace Crpg.WebApi
{
    public class Program
    {
        private static readonly string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                             ?? Environments.Development;

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{Env}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog()
                .Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                var mediator = services.GetRequiredService<IMediator>();
                var eventRaiser = services.GetRequiredService<IEventRaiser>();
                var db = services.GetRequiredService<CrpgDbContext>();

                if (Env == Environments.Production)
                {
                    try
                    {
                        await db.Database.MigrateAsync();
                        var conn = (NpgsqlConnection) db.Database.GetDbConnection();
                        conn.Open();
                        conn.ReloadTypes();
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal(ex, "An error occurred while migrating the database.");
                        Log.CloseAndFlush();
                        return 1;
                    }
                }

                try
                {
                    await mediator.Send(new SeedDataCommand(), CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "An error occurred while initializing the database.");
                    Log.CloseAndFlush();
                    return 1;
                }

                eventRaiser.Raise(EventLevel.Info, "cRPG Web API has started", string.Empty);
            }

            try
            {
                await host.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
