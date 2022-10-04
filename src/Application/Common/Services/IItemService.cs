using Crpg.Application.Common.Interfaces;
using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Services;

internal interface IItemService
{
    /// <summary>Sells a user item. <see cref="UserItem.BaseItem"/> and <see cref="UserItem.EquippedItems"/> should be loaded.</summary>
    void SellUserItem(ICrpgDbContext db, UserItem userItem);
}

internal class ItemService : IItemService
{
    private readonly IItemModifierService _itemModifierService;
    private readonly Constants _constants;

    public ItemService(IItemModifierService itemModifierService, Constants constants)
    {
        _itemModifierService = itemModifierService;
        _constants = constants;
    }

    /// <inheritdoc />
    public void SellUserItem(ICrpgDbContext db, UserItem userItem)
    {
        int price = _itemModifierService.ModifyItem(userItem.BaseItem!, userItem.Rank).Price;
        userItem.User!.Gold += (int)MathHelper.ApplyPolynomialFunction(price, _constants.ItemSellCostCoefs);
        db.EquippedItems.RemoveRange(userItem.EquippedItems);
        db.UserItems.Remove(userItem);
    }
}
