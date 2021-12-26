using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterWeaponProficienciesViewModel : IMapFrom<CharacterWeaponProficiencies>
{
    public int Points { get; init; }
    public int OneHanded { get; init; }
    public int TwoHanded { get; init; }
    public int Polearm { get; init; }
    public int Bow { get; init; }
    public int Throwing { get; init; }
    public int Crossbow { get; init; }
}
