using Crpg.Application;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Crpg.Strategus;
using LoggingHostBuilderExtension = Crpg.Logging.LoggingHostBuilderExtension;

string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
var appEnv = ApplicationEnvironmentProvider.FromEnvironment();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{env}.json")
    .AddEnvironmentVariables()
    .Build();

Crpg.Logging.LoggerFactory.Initialize(configuration);

LoggingHostBuilderExtension.UseLogging(Host.CreateDefaultBuilder(args))
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
