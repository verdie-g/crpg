namespace Crpg.Domain.Entities.Strategus
{
    /// <summary>
    /// Phase of a <see cref="StrategusBattle"/>.
    /// </summary>
    public enum StrategusBattlePhase
    {
        /// <summary>
        /// A <see cref="StrategusHero"/> attacked another one or a <see cref="StrategusSettlement"/>. Other <see cref="StrategusHero"/>es
        /// can join their army during this phase.
        /// </summary>
        Preparation,

        /// <summary>
        /// <see cref="StrategusHero"/>es from anywhere on the map can join the <see cref="StrategusBattle"/> as mercenary.
        /// </summary>
        Hiring,

        /// <summary>
        /// The <see cref="StrategusBattle"/> is live on a server.
        /// </summary>
        Battle,

        /// <summary>
        /// The <see cref="StrategusBattle"/> ended.
        /// </summary>
        End,
    }
}
