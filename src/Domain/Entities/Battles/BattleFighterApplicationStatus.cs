namespace Crpg.Domain.Entities.Battles
{
    /// <summary>
    /// Status of a <see cref="FighterApplication"/>.
    /// </summary>
    public enum BattleFighterApplicationStatus
    {
        /// <summary>
        /// <see cref="FighterApplication"/> is waiting for a response.
        /// </summary>
        Pending,

        /// <summary>
        /// <see cref="FighterApplication"/> was declined.
        /// </summary>
        Declined,

        /// <summary>
        /// <see cref="FighterApplication"/> was accepted.
        /// </summary>
        Accepted,
    }
}
