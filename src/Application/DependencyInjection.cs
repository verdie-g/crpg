using System.Reflection;
using Crpg.Application.Common.Behaviors;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Sdk.Abstractions;
using FluentValidation;
using MaxMind.GeoIP2;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration, IApplicationEnvironment appEnv)
    {
        var constants = new FileConstantsSource().LoadConstants();
        var itemModifiers = new FileItemModifiersSource().LoadItemModifiers();
        ItemModifierService itemModifierService = new(itemModifiers);
        ExperienceTable experienceTable = new(constants);
        CharacterService characterService = new(experienceTable, constants);
        ClanService clanService = new();
        BattleScheduler strategusBattleScheduler = new();

        services.AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestInstrumentationBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
            .AddSingleton<IItemModifierService>(itemModifierService)
            .AddSingleton<IExperienceTable>(experienceTable)
            .AddSingleton<ICharacterService>(characterService)
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IItemService, ItemService>()
            .AddSingleton<IClanService>(clanService)
            .AddSingleton<IActivityLogService, ActivityLogService>()
            .AddSingleton<IGameServerStatsService, DatadogGameServerStatsService>()
            .AddSingleton<IGeoIpService>(CreateGeoIpService())
            .AddSingleton<IStrategusMap, StrategusMap>()
            .AddSingleton<IStrategusSpeedModel, StrategusSpeedModel>()
            .AddSingleton<IBattleScheduler>(strategusBattleScheduler)
            .AddSingleton<ICharacterClassResolver, CharacterClassResolver>()
            .AddSingleton<IBattleMercenaryDistributionModel, BattleMercenaryUniformDistributionModel>()
            .AddSingleton(constants)
            .AddSingleton<IItemsSource, FileItemsSource>()
            .AddSingleton<ISettlementsSource, FileSettlementsSource>()
            .AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }

    private static IGeoIpService CreateGeoIpService()
    {
        const string geoIpDatabasePath = "/usr/share/geoip/GeoLite2-Country.mmdb";
        if (!File.Exists(geoIpDatabasePath))
        {
            return new StubGeoIpService();
        }

        DatabaseReader geoIpDatabase = new(geoIpDatabasePath);
        return new MaxMindGeoIpService(geoIpDatabase);
    }
}
