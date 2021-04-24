using System;
using System.IO;
using Crpg.Application;
using Crpg.Logging;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Crpg.Strategus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
var appEnv = ApplicationEnvironmentProvider.FromEnvironment();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{env}.json")
    .AddEnvironmentVariables()
    .Build();

LoggerFactory.Initialize(configuration);

Host.CreateDefaultBuilder(args)
    .UseLogging()
    .ConfigureServices(services => ConfigureServices(services, configuration, appEnv))
    .Build()
    .Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IApplicationEnvironment appEnv)
{
    services
        .AddSdk(configuration, appEnv)
        .AddPersistence(configuration, appEnv)
        .AddApplication()
        .AddHostedService<Worker>();
}
