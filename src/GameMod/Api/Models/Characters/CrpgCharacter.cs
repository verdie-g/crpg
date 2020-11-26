namespace Crpg.GameMod.Api.Models.Characters
{
    // Copy of Crpg.Application.Characters.Models.CharacterViewModel
    internal class CrpgCharacter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Generation { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public string BodyProperties { get; set; } = string.Empty;
        public CrpgCharacterGender Gender { get; set; }
        public CrpgCharacterStatistics Statistics { get; set; } = new CrpgCharacterStatistics();
        public CrpgCharacterItems Items { get; set; } = new CrpgCharacterItems();
    }
}
