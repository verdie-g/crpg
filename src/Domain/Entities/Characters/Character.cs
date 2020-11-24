using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Characters
{
    /// <summary>
    /// Represents a cRPG character.
    /// </summary>
    public class Character : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Generation { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public float ExperienceMultiplier { get; set; }
        public bool SkippedTheFun { get; set; } // unused (issue #13)

        /// <summary>
        /// A 128 character long string representing the in-game body of the character (height, face, ...).
        /// </summary>
        public string BodyProperties { get; set; } = string.Empty;

        /// <summary>
        /// In-game gender of the character.
        /// </summary>
        public CharacterGender Gender { get; set; }

        public CharacterStatistics Statistics { get; set; } = new CharacterStatistics();
        public CharacterItems Items { get; set; } = new CharacterItems();

        public User? User { get; set; }
    }
}
