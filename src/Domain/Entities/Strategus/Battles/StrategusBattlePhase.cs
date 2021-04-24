namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Phase of a <see cref="StrategusBattle"/>.
    /// </summary>
    public enum StrategusBattlePhase
    {
        /// <summary>
        /// A <see cref="StrategusHero"/> attacked another one or a <see cref="StrategusSettlement"/>. Other <see cref="StrategusHero"/>es
        /// can join their army during this phase as <see cref="StrategusBattleFighter"/>.
        /// </summary>
        Preparation,

        /// <summary>
        /// <see cref="StrategusHero"/>es from anywhere on the map can join the <see cref="StrategusBattle"/>
        /// as <see cref="StrategusBattleMercenary"/>.
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
