namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.EquippedItemViewModel.
/// </summary>
internal class CrpgEquippedItem
{
    public CrpgItem Item { get; set; } = default!;
    public CrpgItemSlot Slot { get; set; }
}
