using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterViewModel : IMapFrom<Character>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Generation { get; init; }
    public int Level { get; init; }
    public int Experience { get; init; }
    public CharacterClass Class { get; init; }
    public bool ForTournament { get; init; }
}
