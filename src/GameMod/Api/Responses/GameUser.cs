using System;

namespace Crpg.GameMod.Api.Responses
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Models.GameUser.
    /// </summary>
    public class GameUser
    {
        public int Id { get; set; }
        public GameCharacter Character { get; set; } = default!;
        public GameBan? Ban { get; set; }
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.GameCharacter.
    /// </summary>
    public class GameCharacter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Generation { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public GameCharacterStatistics Statistics { get; set; } = new GameCharacterStatistics();
        public GameCharacterItems Items { get; set; } = new GameCharacterItems();
    }

    /// <summary>
    /// Copy of Crpg.Application.Characters.Models.CharacterStatisticsViewModel
    /// </summary>
    public class GameCharacterStatistics
    {
        public GameCharacterAttributes Attributes { get; set; } = new GameCharacterAttributes();
        public GameCharacterSkills Skills { get; set; } = new GameCharacterSkills();
        public GameCharacterWeaponProficiencies WeaponProficiencies { get; set; } = new GameCharacterWeaponProficiencies();
    }

    public class GameCharacterAttributes
    {
        public int Points { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
    }

    public class GameCharacterSkills
    {
        public int Points { get; set; }
        public int IronFlesh { get; set; }
        public int PowerStrike { get; set; }
        public int PowerDraw { get; set; }
        public int PowerThrow { get; set; }
        public int Athletics { get; set; }
        public int Riding { get; set; }
        public int WeaponMaster { get; set; }
        public int HorseArchery { get; set; }
        public int Shield { get; set; }
    }

    public class GameCharacterWeaponProficiencies
    {
        public int Points { get; set; }
        public int OneHanded { get; set; }
        public int TwoHanded { get; set; }
        public int Polearm { get; set; }
        public int Bow { get; set; }
        public int Throwing { get; set; }
        public int Crossbow { get; set; }
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.GameCharacterItems.
    /// </summary>
    public class GameCharacterItems
    {
        public string? HeadItemMbId { get; set; }
        public string? CapeItemMbId { get; set; }
        public string? BodyItemMbId { get; set; }
        public string? HandItemMbId { get; set; }
        public string? LegItemMbId { get; set; }
        public string? HorseHarnessItemMbId { get; set; }
        public string? HorseItemMbId { get; set; }
        public string? Weapon1ItemMbId { get; set; }
        public string? Weapon2ItemMbId { get; set; }
        public string? Weapon3ItemMbId { get; set; }
        public string? Weapon4ItemMbId { get; set; }
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.GameBan.
    /// </summary>
    public class GameBan
    {
        public DateTimeOffset Until { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}