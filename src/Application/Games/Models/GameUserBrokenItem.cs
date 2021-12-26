namespace Crpg.Application.Games.Models;

public record GameUserBrokenItem
{
    public int ItemId { get; init; }
    public int RepairCost { get; init; }
}
