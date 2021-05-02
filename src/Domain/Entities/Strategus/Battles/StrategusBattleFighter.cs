namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Fighter that joined their army to a <see cref="StrategusBattle"/> during the <see cref="StrategusBattlePhase.Preparation"/>
    /// phase. Fighter can be either a <see cref="StrategusHero"/> or a <see cref="StrategusSettlement"/>. Not to be
    /// confused with <see cref="StrategusBattleMercenary"/>.
    /// </summary>
    public class StrategusBattleFighter
    {
        public int Id { get; set; }
        public int BattleId { get; set; }

        /// <summary>
        /// The id of the hero that joined the <see cref="StrategusBattle"/>. Null if <see cref="StrategusBattleFighter"/>
        /// represents a <see cref="StrategusSettlement"/>.
        /// </summary>
        public int? HeroId { get; set; }

        /// <summary>
        /// The id of the settlement that being siege in the <see cref="StrategusBattle"/>. Null if <see cref="StrategusBattleFighter"/>
        /// represents a <see cref="StrategusHero"/>.
        /// </summary>
        public int? SettlementId { get; set; }

        public StrategusBattleSide Side { get; set; }

        /// <summary>
        /// Is the <see cref="StrategusBattleFighter"/> the fighter that initiated the <see cref="StrategusBattle"/>.
        /// There is one main fighter by <see cref="StrategusBattleSide"/>. If <see cref="SettlementId"/> is not null
        /// then it is guarantee that <see cref="MainFighter"/> is true.
        /// </summary>
        public bool MainFighter { get; set; }

        public StrategusHero? Hero { get; set; }
        public StrategusSettlement? Settlement { get; set; }
        public StrategusBattle? Battle { get; set; }
    }
}
