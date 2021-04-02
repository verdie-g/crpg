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
                    return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                int[] newItemIds = req.Items
                    .Where(i => i.ItemId != null)
                    .Select(i => i.ItemId!.Value).ToArray();
                Dictionary<int, OwnedItem> ownedItemsById = await _db.OwnedItems
                    .Include(oi => oi.Item)
                    .Where(oi => oi.UserId == req.UserId && newItemIds.Contains(oi.ItemId))
                    .ToDictionaryAsync(oi => oi.ItemId, cancellationToken);

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

                    if (!ownedItemsById.TryGetValue(newItem.ItemId.Value, out OwnedItem? ownedItem))
                    {
                        return new(CommonErrors.ItemNotOwned(newItem.ItemId.Value));
                    }

                    if (!ItemSlotsByType[ownedItem.Item!.Type].Contains(newItem.Slot))
                    {
                        return new(CommonErrors.ItemBadSlot(newItem.ItemId.Value, newItem.Slot));
                    }

                    if (oldItemsBySlot.TryGetValue(newItem.Slot, out equippedItem))
                    {
                        // Character already has an item in this slot. Replace it.
                        equippedItem.OwnedItem = ownedItem;
                    }
                    else
                    {
                        // Character has no item in this slot. Create it.
                        equippedItem = new EquippedItem
                        {
                            CharacterId = req.CharacterId,
                            Slot = newItem.Slot,
                            OwnedItem = ownedItem,
                        };

                        character.EquippedItems.Add(equippedItem);
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new(_mapper.Map<IList<EquippedItemViewModel>>(character.EquippedItems));
            }
        }
    }
}
