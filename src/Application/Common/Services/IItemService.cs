using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Items;
using Crpg.Sdk.Abstractions;

namespace Crpg.Application.Common.Services;

internal interface IItemService
{
    /// <summary>Sells a user item. <see cref="UserItem.Item"/> and <see cref="UserItem.EquippedItems"/> should be loaded.</summary>
    int SellUserItem(ICrpgDbContext db, UserItem userItem);
}

internal class ItemService : IItemService
{
    private readonly IDateTime _dateTime;
    private readonly Constants _constants;

    public ItemService(IDateTime dateTime, Constants constants)
    {
        _dateTime = dateTime;
        _constants = constants;
    }

    /// <inheritdoc />
    public int SellUserItem(ICrpgDbContext db, UserItem userItem)
    {
        int price = userItem.Item!.Price;
        // If the item was recently bought it is sold at 100% of its original price.
        int sellPrice = userItem.CreatedAt + TimeSpan.FromHours(1) < _dateTime.UtcNow
            ? (int)(price * _constants.ItemSellCostPenalty)
            : price;
        userItem.User!.Gold += sellPrice;
        db.EquippedItems.RemoveRange(userItem.EquippedItems);
        db.UserItems.Remove(userItem);

        return sellPrice;
    }
}
