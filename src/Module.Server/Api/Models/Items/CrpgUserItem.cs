namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.UserItemViewModel.
/// </summary>
internal class CrpgUserItem
{
    public int Id { get; set; }
    public CrpgItem BaseItem { get; set; } = default!;
    public int Rank { get; set; }
}
