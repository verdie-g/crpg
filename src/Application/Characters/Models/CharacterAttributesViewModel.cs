using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Models
{
    public class CharacterAttributesViewModel : IMapFrom<CharacterAttributes>
    {
        public int Points { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
    }
}