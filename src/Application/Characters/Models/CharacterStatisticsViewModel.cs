using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public class CharacterStatisticsViewModel : IMapFrom<CharacterStatistics>
    {
        public CharacterAttributesViewModel Attributes { get; set; } = new CharacterAttributesViewModel();
        public CharacterSkillsViewModel Skills { get; set; } = new CharacterSkillsViewModel();
        public CharacterWeaponProficienciesViewModel WeaponProficiencies { get; set; } = new CharacterWeaponProficienciesViewModel();
    }
}