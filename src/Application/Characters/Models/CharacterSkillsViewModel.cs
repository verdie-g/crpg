using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public class CharacterSkillsViewModel : IMapFrom<CharacterSkills>
    {
        public int Points { get; set; }
        public int IronFlesh { get; set; }
        public int PowerStrike { get; set; }
        public int PowerDraw { get; set; }
        public int PowerThrow { get; set; }
        public int Athletics { get; set; }
        public int Riding { get; set; }
        public int WeaponMaster { get; set; }
        public int HorseArchery { get; set; }
        public int Shield { get; set; }
    }
}