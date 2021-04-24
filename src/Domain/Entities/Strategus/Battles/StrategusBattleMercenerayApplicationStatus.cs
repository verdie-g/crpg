namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Status of a <see cref="StrategusBattleMercenaryApplication"/>.
    /// </summary>
    public enum StrategusBattleMercenaryApplicationStatus
    {
        /// <summary>
        /// <see cref="StrategusBattleMercenaryApplication"/> is waiting for a response.
        /// </summary>
        Pending,

        /// <summary>
        /// <see cref="StrategusBattleMercenaryApplication"/> was declined.
        /// </summary>
        Declined,

        /// <summary>
        /// <see cref="StrategusBattleMercenaryApplication"/> was accepted.
        /// </summary>
        Accepted,
    }
}
