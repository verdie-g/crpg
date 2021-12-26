using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// User that joined a <see cref="Battle"/> as an individual with their character. Not to be confused with <see cref="BattleFighter"/>.
/// </summary>
public class BattleMercenary
{
    public int Id { get; set; }

    /// <summary>The id of the battle the user will fight in.</summary>
    public int BattleId { get; set; }

    /// <summary>The id of the character the user will fight with.</summary>
    public int CharacterId { get; set; }
    public BattleSide Side { get; set; }

    /// <summary>The id of the fighter the user will fight for.</summary>
    public int CaptainFighterId { get; set; }

    /// <summary>The id of the application that got the user accepted in the battle.</summary>
    public int ApplicationId { get; set; }

    /// <summary>See <see cref="CharacterId"/>.</summary>
    public Character? Character { get; set; }

    /// <summary>See <see cref="BattleId"/>.</summary>
    public Battle? Battle { get; set; }

    /// <summary>See <see cref="CaptainFighterId"/>.</summary>
    public BattleFighter? CaptainFighter { get; set; }

    /// <summary>See <see cref="ApplicationId"/>.</summary>
    public BattleMercenaryApplication? Application { get; set; }
}
