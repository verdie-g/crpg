namespace Crpg.Application.Games.Models;

public record GameUserDamagedItem
{
    public int UserItemId { get; init; }
    public int RepairCost { get; init; }
}
