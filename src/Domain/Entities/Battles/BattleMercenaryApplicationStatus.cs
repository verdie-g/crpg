namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Status of a <see cref="BattleMercenaryApplication"/>.
/// </summary>
public enum BattleMercenaryApplicationStatus
{
    /// <summary>
    /// <see cref="BattleMercenaryApplication"/> is waiting for a response.
    /// </summary>
    Pending,

    /// <summary>
    /// <see cref="BattleMercenaryApplication"/> was declined.
    /// </summary>
    Declined,

    /// <summary>
    /// <see cref="BattleMercenaryApplication"/> was accepted.
    /// </summary>
    Accepted,
}
