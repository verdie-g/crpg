namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameUserItemViewModel.
/// </summary>
internal class CrpgUserItem
{
    public int Id { get; set; }
    public string BaseItemId { get; set; } = default!;
    public int BrokenState { get; set; }
}
