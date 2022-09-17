namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameEquippedItemViewModel.
/// </summary>
internal class CrpgEquippedItem
{
    public CrpgItemSlot Slot { get; set; }
    public CrpgUserItem UserItem { get; set; } = default!;
}
