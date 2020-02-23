using AutoMapper;

namespace Crpg.Application.Common.Mappings
{
    /// <summary>
    /// Indicates that the type is mappable from <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type of the mapping.</typeparam>
    public interface IMapFrom<TSource>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(TSource), GetType());
    }
}