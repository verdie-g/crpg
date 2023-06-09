using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameUserItemViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public string ItemId { get; init; } = default!;
}
