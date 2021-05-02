using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusBattleMercenaryViewModel
    {
        public int Id { get; set; }
        public UserPublicViewModel User { get; init; } = default!;
        public CharacterPublicViewModel Character { get; init; } = default!;
        public StrategusBattleSide Side { get; set; }
    }
}
