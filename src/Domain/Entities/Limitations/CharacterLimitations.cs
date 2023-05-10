using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Limitations;

public class CharacterLimitations
{
    public int CharacterId { get; set; }
    public DateTime LastRespecializeAt { get; set; }

    public Character? Character { get; set; }
}
