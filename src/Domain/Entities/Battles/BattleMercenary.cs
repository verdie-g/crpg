using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Battles
{
    /// <summary>
    /// User that joined a <see cref="Battle"/> as an individual with their character. Not to be confused with <see cref="BattleFighter"/>.
    /// </summary>
    public class BattleMercenary
    {
        public int Id { get; set; }
        public int BattleId { get; set; }

        /// <summary>The id of the character the user will fight with.</summary>
        public int CharacterId { get; set; }
        public BattleSide Side { get; set; }
        public int CaptainFighterId { get; set; }
        public int ApplicationId { get; set; }

        public Character? Character { get; set; }
        public Battle? Battle { get; set; }
        public BattleFighter? CaptainFighter { get; set; }
        public BattleMercenaryApplication? Application { get; set; }
    }
}
