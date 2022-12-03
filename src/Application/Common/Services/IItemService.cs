using Crpg.Application.Common.Interfaces;
using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Items;
using Crpg.Sdk.Abstractions;

namespace Crpg.Application.Common.Services;

internal interface IItemService
{
    /// <summary>Sells a user item. <see cref="UserItem.BaseItem"/> and <see cref="UserItem.EquippedItems"/> should be loaded.</summary>
    void SellUserItem(ICrpgDbContext db, UserItem userItem);
}

internal class ItemService : IItemService
{
    private readonly IItemModifierService _itemModifierService;
    private readonly IDateTime _dateTime;
    private readonly Constants _constants;

    public ItemService(IItemModifierService itemModifierService, IDateTime dateTime, Constants constants)
    {
        _itemModifierService = itemModifierService;
        _dateTime = dateTime;
        _constants = constants;
    }

    /// <inheritdoc />
    public void SellUserItem(ICrpgDbContext db, UserItem userItem)
    {
        int price = _itemModifierService.ModifyItem(userItem.BaseItem!, userItem.Rank).Price;
        // If the item was recently bought it is sold at 100% of its original price.
        userItem.User!.Gold += userItem.CreatedAt + TimeSpan.FromHours(1) < _dateTime.UtcNow
            ? (int)MathHelper.ApplyPolynomialFunction(price, _constants.ItemSellCostCoefs)
            : price;
        db.EquippedItems.RemoveRange(userItem.EquippedItems);
        db.UserItems.Remove(userItem);
    }
}
