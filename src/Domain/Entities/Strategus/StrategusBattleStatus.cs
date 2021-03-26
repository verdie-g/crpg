namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Status of a <see cref="StrategusBattle"/>.
    /// </summary>
    public enum StrategusBattleStatus
    {
        /// <summary>
        /// A <see cref="StrategusHero"/> attacked another one or a <see cref="StrategusSettlement"/>. Other <see cref="StrategusHero"/>es
        /// can join their army during this phase.
        /// </summary>
        Initiated,

        /// <summary>
        /// <see cref="StrategusHero"/>es from anywhere on the map can join the <see cref="StrategusBattle"/> as mercenary.
        /// </summary>
        Hiring,

        /// <summary>
        /// The <see cref="StrategusBattle"/> is live on a server.
        /// </summary>
        Live,

        /// <summary>
        /// The <see cref="StrategusBattle"/> ended.
        /// </summary>
        Ended,
    }
}
