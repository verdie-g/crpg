using System.Reflection;
using AutoMapper;

namespace Crpg.Application.Common.Mappings;

/// <summary>
/// AutoMapper's profile. Used by IServiceCollection.AddAutoMapper.
/// </summary>
public class MappingProfile : Profile
{
    private const string MappingMethodName = nameof(IMapFrom<object>.Mapping);
    private static readonly Type MapFromInterfaceType = typeof(IMapFrom<>);

    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Calls <see cref="IMapFrom{TSource}.Mapping"/>(this) on all types implementing <see cref="IMapFrom{TSource}"/>.
    /// </summary>
    /// <param name="assembly">Assembly to scan.</param>
    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            var interfaces = type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == MapFromInterfaceType)
                .ToList();

            if (interfaces.Count == 0)
            {
                continue;
            }

            object? instance = Activator.CreateInstance(type);
            foreach (var i in interfaces)
            {
                var methodInfo = i.GetMethod(MappingMethodName);
                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
