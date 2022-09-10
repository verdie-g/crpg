using Crpg.Application.Characters.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleMercenaryViewModel
{
    public int Id { get; init; }
    public UserPublicViewModel User { get; init; } = default!;
    public CharacterPublicViewModel Character { get; init; } = default!;
    public BattleSide Side { get; init; }
}
