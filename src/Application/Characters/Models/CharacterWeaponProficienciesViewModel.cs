using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Models
{
    public class CharacterWeaponProficienciesViewModel : IMapFrom<CharacterWeaponProficiencies>
    {
        public int Points { get; set; }
        public int OneHanded { get; set; }
        public int TwoHanded { get; set; }
        public int Polearm { get; set; }
        public int Bow { get; set; }
        public int Throwing { get; set; }
        public int Crossbow { get; set; }
    }
}