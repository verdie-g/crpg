namespace Crpg.Application.Games.Models;

public record GameRepairedItem
{
    public string ItemId { get; init; } = string.Empty;
    public int RepairCost { get; init; }
    public bool Broke { get; init; }
}
