using Crpg.Domain.Common;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Application to join a <see cref="Battle"/> during the <see cref="BattlePhase.Preparation"/>
/// phase.
/// </summary>
public class BattleMercenaryApplication : AuditableEntity
{
    public int Id { get; set; }
    public int BattleId { get; set; }
    public int CharacterId { get; set; }

    /// <summary>The side the character is applying to.</summary>
    public BattleSide Side { get; set; }

    /// <summary>Amount of gold the mercenary is requesting.</summary>
    public int Wage { get; set; }

    /// <summary>A note to the recruiters.</summary>
    public string Note { get; set; } = string.Empty;
    public BattleMercenaryApplicationStatus Status { get; set; }

    public Battle? Battle { get; set; }
    public Character? Character { get; set; }
}
