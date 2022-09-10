namespace Crpg.Application.Characters.Models;

public record CharacterPublicViewModel
{
    public int Id { get; init; }
    public int Level { get; init; }
    public CharacterClass Class { get; init; }
}
