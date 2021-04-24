using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Strategus.Battles
{
    /// <summary>
    /// User that joined a <see cref="StrategusBattle"/> as an individual with their character. Not to be confused
    /// with <see cref="StrategusBattleFighter"/>.
    /// </summary>
    public class StrategusBattleMercenary
    {
        public int Id { get; set; }
        public int BattleId { get; set; }

        /// <summary>The id of the character the user will fight with.</summary>
        public int CharacterId { get; set; }
        public StrategusBattleSide Side { get; set; }

        public Character? Character { get; set; }
        public StrategusBattle? Battle { get; set; }
    }
}
