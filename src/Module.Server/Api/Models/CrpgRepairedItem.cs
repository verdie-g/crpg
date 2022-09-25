namespace Crpg.Module.Api.Models;

// Copy of Crpg.Application.Games.Models.GameRepairedItem
internal class CrpgRepairedItem
{
    public string ItemId { get; set; } = string.Empty;
    public int RepairCost { get; set; }
    public bool Sold { get; set; }
}
