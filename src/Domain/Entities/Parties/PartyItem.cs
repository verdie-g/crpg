using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// Item owned by a party. Similar to <see cref="SettlementItem"/> but for <see cref="Party"/>.
/// </summary>
public class PartyItem : AuditableEntity
{
    public int PartyId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int Count { get; set; }

    public Party? Party { get; set; }
    public Item? Item { get; set; }
}
