using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Characters;

/// <summary>
/// Represents a cRPG character.
/// </summary>
public class Character : AuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Generation { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public CharacterClass Class { get; set; }

    /// <summary>
    /// True if the character is used for for tournaments.
    /// </summary>
    public bool ForTournament { get; set; }

    /// <summary>
    /// Not null if the character was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public CharacterCharacteristics Characteristics { get; set; } = new();
    public IList<EquippedItem> EquippedItems { get; set; } = new List<EquippedItem>();
    public CharacterStatistics Statistics { get; set; } = new();
    public CharacterRating Rating { get; set; } = new();

    public User? User { get; set; }
}
