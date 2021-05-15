using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Battles
{
    /// <summary>
    /// Fighter that joined their army to a <see cref="Battle"/> during the <see cref="BattlePhase.Preparation"/>
    /// phase. Fighter can be either a <see cref="Hero"/> or a <see cref="Settlement"/>. Not to be confused with
    /// <see cref="BattleMercenary"/>.
    /// </summary>
    public class BattleFighter
    {
        public int Id { get; set; }
        public int BattleId { get; set; }

        /// <summary>
        /// The id of the hero that joined the <see cref="Battle"/>. Null if <see cref="BattleFighter"/>
        /// represents a <see cref="Settlement"/>.
        /// </summary>
        public int? HeroId { get; set; }

        /// <summary>
        /// The id of the settlement that being siege in the <see cref="Battle"/>. Null if <see cref="BattleFighter"/>
        /// represents a <see cref="Hero"/>.
        /// </summary>
        public int? SettlementId { get; set; }

        public BattleSide Side { get; set; }

        /// <summary>
        /// Is the <see cref="BattleFighter"/> the fighter that initiated the <see cref="Battle"/>. There is one commander
        /// by <see cref="BattleSide"/>. If <see cref="SettlementId"/> is not null then it is guarantee that
        /// <see cref="Commander"/> is true.
        /// </summary>
        public bool Commander { get; set; }

        public Hero? Hero { get; set; }
        public Settlement? Settlement { get; set; }
        public Battle? Battle { get; set; }
    }
}
