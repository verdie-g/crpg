using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public record CharacterAttributesViewModel : IMapFrom<CharacterAttributes>
    {
        public int Points { get; init; }
        public int Strength { get; init; }
        public int Agility { get; init; }
    }
}
