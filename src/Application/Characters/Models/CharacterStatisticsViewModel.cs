using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public record CharacterStatisticsViewModel : IMapFrom<CharacterStatistics>
    {
        public CharacterAttributesViewModel Attributes { get; init; } = new();
        public CharacterSkillsViewModel Skills { get; init; } = new();
        public CharacterWeaponProficienciesViewModel WeaponProficiencies { get; init; } = new();
    }
}
