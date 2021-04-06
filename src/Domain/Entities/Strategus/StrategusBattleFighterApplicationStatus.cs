namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Status of a <see cref="StrategusBattleFighterApplication"/>.
    /// </summary>
    public enum StrategusBattleFighterApplicationStatus
    {
        /// <summary>
        /// <see cref="StrategusBattleFighterApplication"/> is waiting for a response.
        /// </summary>
        Pending,

        /// <summary>
        /// <see cref="StrategusBattleFighterApplication"/> was declined.
        /// </summary>
        Declined,

        /// <summary>
        /// <see cref="StrategusBattleFighterApplication"/> was accepted.
        /// </summary>
        Accepted,
    }
}
