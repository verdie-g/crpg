namespace Crpg.Application.Games.Models;

public record GameUserBrokenItem
{
    public int UserItemId { get; init; }
    public int RepairCost { get; init; }
}
