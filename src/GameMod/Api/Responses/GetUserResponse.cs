using System;

namespace Crpg.GameMod.Api.Responses
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Models.GameUser.
    /// </summary>
    public class GetUserResponse
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
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Level { get; set; }
        public GameCharacterItems Items { get; set; } = new GameCharacterItems();
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