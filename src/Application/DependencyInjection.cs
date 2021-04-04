using System.Reflection;
using Crpg.Application.Common.Behaviors;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var constants = new FileConstantsSource().LoadConstants();
            var itemModifiers = new FileItemModifiersSource().LoadItemModifiers();
            var itemModifierService = new ItemModifierService(itemModifiers);
            var experienceTable = new ExperienceTable(constants);
            var characterService = new CharacterService(experienceTable, constants);
            var userService = new UserService(constants);
            var clanService = new ClanService();
            var strategusBattleScheduler = new StrategusBattleScheduler();

            services.AddAutoMapper(Assembly.GetExecutingAssembly())
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestInstrumentationBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddSingleton(typeof(RequestMetrics<>))
                .AddSingleton(itemModifierService)
                .AddSingleton<ItemValueModel>()
                .AddSingleton<IExperienceTable>(experienceTable)
                .AddSingleton<ICharacterService>(characterService)
                .AddSingleton<IUserService>(userService)
                .AddSingleton<IClanService>(clanService)
                .AddSingleton<IStrategusMap, StrategusMap>()
                .AddSingleton<IStrategusBattleScheduler>(strategusBattleScheduler)
                .AddSingleton(constants)
                .AddSingleton<IItemsSource, FileItemsSource>()
                .AddSingleton<IStrategusSettlementsSource, FileStrategusSettlementsSource>()
                .AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

            return services;
        }
    }
}
