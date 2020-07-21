using System.Reflection;
using AutoMapper;
using Crpg.Application.Common.Behaviors;
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
            services.AddAutoMapper(Assembly.GetExecutingAssembly())
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestInstrumentationBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddSingleton(typeof(RequestMetrics<>))
                .AddSingleton<ItemModifierService>()
                .AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

            return services;
        }
    }
}