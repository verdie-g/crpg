using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands;

public record UpdateCharacterItemsCommand : IMediatorRequest<IList<EquippedItemViewModel>>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }
    public IList<EquippedItemIdViewModel> Items { get; init; } = Array.Empty<EquippedItemIdViewModel>();

    internal class Handler : IMediatorRequestHandler<UpdateCharacterItemsCommand, IList<EquippedItemViewModel>>
    {
        private static readonly ItemSlot[] WeaponSlots =
        {
            ItemSlot.Weapon0,
            ItemSlot.Weapon1,
            ItemSlot.Weapon2,
            ItemSlot.Weapon3,
        };

        private static readonly Dictionary<ItemType, ItemSlot[]> ItemSlotsByType = new()
        {
            [ItemType.HeadArmor] = new[] { ItemSlot.Head },
            [ItemType.ShoulderArmor] = new[] { ItemSlot.Shoulder },
            [ItemType.BodyArmor] = new[] { ItemSlot.Body },
            [ItemType.HandArmor] = new[] { ItemSlot.Hand },
            [ItemType.LegArmor] = new[] { ItemSlot.Leg },
            [ItemType.MountHarness] = new[] { ItemSlot.MountHarness },
            [ItemType.Mount] = new[] { ItemSlot.Mount },
            [ItemType.Shield] = WeaponSlots,
            [ItemType.Bow] = WeaponSlots,
            [ItemType.Crossbow] = WeaponSlots,
            [ItemType.OneHandedWeapon] = WeaponSlots,
            [ItemType.TwoHandedWeapon] = WeaponSlots,
            [ItemType.Polearm] = WeaponSlots,
            [ItemType.Thrown] = WeaponSlots,
            [ItemType.Arrows] = WeaponSlots,
            [ItemType.Bolts] = WeaponSlots,
            [ItemType.Pistol] = WeaponSlots,
            [ItemType.Musket] = WeaponSlots,
            [ItemType.Bullets] = WeaponSlots,
            [ItemType.Banner] = new[] { ItemSlot.WeaponExtra },
        };

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<EquippedItemViewModel>>> Handle(UpdateCharacterItemsCommand req,
            CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.EquippedItems).ThenInclude(ei => ei.UserItem!.BaseItem)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            int[] newUserItemIds = req.Items
                .Where(ei => ei.UserItemId != null)
                .Select(ei => ei.UserItemId!.Value)
                .ToArray();
            Dictionary<int, UserItem> userItemsById = await _db.UserItems
                .Include(ui => ui.BaseItem)
                .Where(ui => ui.UserId == req.UserId && newUserItemIds.Contains(ui.Id))
                .ToDictionaryAsync(ui => ui.Id, cancellationToken);

            Dictionary<ItemSlot, EquippedItem> oldItemsBySlot = character.EquippedItems.ToDictionary(c => c.Slot);

            foreach (EquippedItemIdViewModel newEquippedItem in req.Items)
            {
                EquippedItem? equippedItem;
                if (newEquippedItem.UserItemId == null) // If null, remove item in the slot.
                {
                    if (oldItemsBySlot.TryGetValue(newEquippedItem.Slot, out equippedItem))
                    {
                        character.EquippedItems.Remove(equippedItem);
                    }

                    continue;
                }

                if (!userItemsById.TryGetValue(newEquippedItem.UserItemId.Value, out UserItem? userItem))
                {
                    return new(CommonErrors.UserItemNotFound(newEquippedItem.UserItemId.Value));
                }

                if (!userItem.BaseItem!.Enabled)
                {
                    return new(CommonErrors.ItemDisabled(userItem.BaseItemId));
                }

                if (userItem.Rank < 0)
                {
                    return new(CommonErrors.ItemBroken(userItem.BaseItemId));
                }

                if ((userItem.BaseItem!.Flags & (ItemFlags.DropOnAnyAction | ItemFlags.DropOnWeaponChange)) != 0)
                {
                    if (newEquippedItem.Slot != ItemSlot.WeaponExtra)
                    {
                        return new(CommonErrors.ItemBadSlot(userItem.BaseItemId, newEquippedItem.Slot));
                    }
                }
                else if (!ItemSlotsByType[userItem.BaseItem!.Type].Contains(newEquippedItem.Slot))
                {
                    return new(CommonErrors.ItemBadSlot(userItem.BaseItemId, newEquippedItem.Slot));
                }

                if (oldItemsBySlot.TryGetValue(newEquippedItem.Slot, out equippedItem))
                {
                    // Character already has an item in this slot. Replace it.
                    equippedItem.UserItem = userItem;
                }
                else
                {
                    // Character has no item in this slot. Create it.
                    equippedItem = new EquippedItem
                    {
                        CharacterId = req.CharacterId,
                        Slot = newEquippedItem.Slot,
                        UserItem = userItem,
                    };

                    character.EquippedItems.Add(equippedItem);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<IList<EquippedItemViewModel>>(character.EquippedItems));
        }
    }
}
