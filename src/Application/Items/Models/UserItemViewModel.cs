using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public ItemViewModel Item { get; init; } = default!;
    public int BrokenState { get; init; }
    public DateTime CreatedAt { get; init; }
}
