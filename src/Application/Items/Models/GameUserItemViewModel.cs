using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameUserItemViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public string BaseItemId { get; init; } = default!;
    public int Rank { get; init; }
}
