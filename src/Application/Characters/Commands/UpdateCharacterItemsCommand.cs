using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterItemsCommand : IMediatorRequest<IList<EquippedItemViewModel>>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public IList<EquippedItemIdViewModel> Items { get; set; } = Array.Empty<EquippedItemIdViewModel>();

        public class Handler : IMediatorRequestHandler<UpdateCharacterItemsCommand, IList<EquippedItemViewModel>>
        {
            private static readonly ItemSlot[] WeaponSlots =
            {
                ItemSlot.Weapon0,
                ItemSlot.Weapon1,
                ItemSlot.Weapon2,
                ItemSlot.Weapon3,
            };

            private static readonly Dictionary<ItemType, ItemSlot[]> ItemSlotsByType = new Dictionary<ItemType, ItemSlot[]>
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
                [ItemType.Banner] = WeaponSlots,
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
                    .Include(c => c.EquippedItems).ThenInclude(ci => ci.Item)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

                if (character == null)
                {
                    return new Result<IList<EquippedItemViewModel>>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                int[] newItemIds = req.Items
                    .Where(i => i.ItemId != null)
                    .Select(i => i.ItemId!.Value).ToArray();
                Dictionary<int, UserItem> userItemsById = await _db.UserItems
                    .Include(oi => oi.Item)
                    .Where(ui => ui.UserId == req.UserId && newItemIds.Contains(ui.ItemId))
                    .ToDictionaryAsync(ui => ui.ItemId, cancellationToken);

                Dictionary<ItemSlot, EquippedItem> oldItemsBySlot = character.EquippedItems.ToDictionary(c => c.Slot);

                foreach (EquippedItemIdViewModel newItem in req.Items)
                {
                    EquippedItem? equippedItem;
                    if (newItem.ItemId == null) // If null, remove item in the slot.
                    {
                        if (oldItemsBySlot.TryGetValue(newItem.Slot, out equippedItem))
                        {
                            character.EquippedItems.Remove(equippedItem);
                        }

                        continue;
                    }

                    if (!userItemsById.TryGetValue(newItem.ItemId.Value, out UserItem? userItem))
                    {
                        return new Result<IList<EquippedItemViewModel>>(CommonErrors.ItemNotOwned(newItem.ItemId.Value));
                    }

                    if (!ItemSlotsByType[userItem.Item!.Type].Contains(newItem.Slot))
                    {
                        return new Result<IList<EquippedItemViewModel>>(CommonErrors.ItemBadSlot(newItem.ItemId.Value, newItem.Slot));
                    }

                    if (oldItemsBySlot.TryGetValue(newItem.Slot, out equippedItem))
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
                            Slot = newItem.Slot,
                            UserItem = userItem
                        };

                        character.EquippedItems.Add(equippedItem);
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<IList<EquippedItemViewModel>>(_mapper.Map<IList<EquippedItemViewModel>>(character.EquippedItems));
            }
        }
    }
}
