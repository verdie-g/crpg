namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// Fighter that joined their army to a <see cref="StrategusBattle"/>. Not to be confused
    /// with <see cref="StrategusBattleMercenary"/>.
    /// </summary>
    public class StrategusBattleFighter
    {
        public int Id { get; set; }
        public int BattleId { get; set; }
        public int HeroId { get; set; }
        public StrategusBattleSide Side { get; set; }

        /// <summary>
        /// Is the <see cref="StrategusBattleFighter"/> the fighter that initiated the <see cref="StrategusBattle"/>.
        /// There is one main fighter by <see cref="StrategusBattleSide"/> or only to the attacking side if they are
        /// attacking a <see cref="StrategusSettlement"/>.
        /// </summary>
        public bool MainFighter { get; set; }

        public StrategusHero? Hero { get; set; }
        public StrategusBattle? Battle { get; set; }
    }
}
