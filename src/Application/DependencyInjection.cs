using System.Reflection;
using Crpg.Application.Common.Behaviors;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
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
            .AddSingleton<IGameServerStatsService, DatadogGameServerStatsService>()
            .AddSingleton<IStrategusMap, StrategusMap>()
            .AddSingleton<IStrategusSpeedModel, StrategusSpeedModel>()
            .AddSingleton<IBattleScheduler>(strategusBattleScheduler)
            .AddSingleton<ICharacterClassModel, CharacterClassModel>()
            .AddSingleton<IBattleMercenaryDistributionModel, BattleMercenaryUniformDistributionModel>()
            .AddSingleton(constants)
            .AddSingleton<IItemsSource, FileItemsSource>()
            .AddSingleton<ISettlementsSource, FileSettlementsSource>()
            .AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
