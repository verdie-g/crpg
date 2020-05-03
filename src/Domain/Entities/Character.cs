using Crpg.Domain.Common;

namespace Crpg.Domain.Entities
{
    public class Character : AuditableEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int Level { get; set; }
        public float ExperienceMultiplier { get; set; }
        public CharacterStatistics Statistics { get; set; } = new CharacterStatistics();
        public CharacterItems Items { get; set; } = new CharacterItems();

        public User? User { get; set; }
    }
}