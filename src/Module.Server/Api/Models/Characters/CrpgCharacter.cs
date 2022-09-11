using Crpg.Module.Api.Models.Items;

namespace Crpg.Module.Api.Models.Characters;

// Copy of Crpg.Application.Characters.Models.CharacterViewModel
internal class CrpgCharacter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Generation { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public float ExperienceMultiplier { get; set; }
    public float Rating { get; set; }
    public float RatingDeviation { get; set; }
    public float Volatility { get; set; }

    public bool SkippedTheFun { get; set; }
    public bool AutoRepair { get; set; }
    public CrpgCharacterCharacteristics Characteristics { get; set; } = new();
    public IList<CrpgEquippedItem> EquippedItems { get; set; } = Array.Empty<CrpgEquippedItem>();
}
