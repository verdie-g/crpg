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
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterItemsCommand : IMediatorRequest<CharacterItemsViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public int? HeadItemId { get; set; }
        public int? CapeItemId { get; set; }
        public int? BodyItemId { get; set; }
        public int? HandItemId { get; set; }
        public int? LegItemId { get; set; }
        public int? HorseHarnessItemId { get; set; }
        public int? HorseItemId { get; set; }
        public int? Weapon1ItemId { get; set; }
        public int? Weapon2ItemId { get; set; }
        public int? Weapon3ItemId { get; set; }
        public int? Weapon4ItemId { get; set; }
        public bool AutoRepair { get; set; }

        public class Handler : IMediatorRequestHandler<UpdateCharacterItemsCommand, CharacterItemsViewModel>
        {
            private static readonly ISet<ItemType> WeaponTypes = new HashSet<ItemType>
            {
                ItemType.Arrows,
                ItemType.Bolts,
                ItemType.Bow,
                ItemType.Crossbow,
                ItemType.OneHandedWeapon,
                ItemType.Polearm,
                ItemType.Shield,
                ItemType.Thrown,
                ItemType.TwoHandedWeapon,
            };

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterItemsViewModel>> Handle(UpdateCharacterItemsCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.Items.HeadItem)
                    .Include(c => c.Items.CapeItem)
                    .Include(c => c.Items.BodyItem)
                    .Include(c => c.Items.HandItem)
                    .Include(c => c.Items.LegItem)
                    .Include(c => c.Items.HorseHarnessItem)
                    .Include(c => c.Items.HorseItem)
                    .Include(c => c.Items.Weapon1Item)
                    .Include(c => c.Items.Weapon2Item)
                    .Include(c => c.Items.Weapon3Item)
                    .Include(c => c.Items.Weapon4Item)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

                if (character == null)
                {
                    return new Result<CharacterItemsViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                try
                {
                    await UpdateCharacterItems(req, character.Items);
                }
                catch (ItemNotOwnedException e)
                {
                    return new Result<CharacterItemsViewModel>(CommonErrors.ItemNotOwned(e.ItemId));
                }
                catch (ItemBadTypeException e)
                {
                    return new Result<CharacterItemsViewModel>(CommonErrors.ItemBadType(e.ItemId, e.ExpectedTypes, e.ActualType));
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterItemsViewModel>(_mapper.Map<CharacterItemsViewModel>(character.Items));
            }

            private async Task UpdateCharacterItems(UpdateCharacterItemsCommand request, CharacterItems characterItems)
            {
                var ids = BuildItemIdCollection(request);
                var itemsById = await _db.UserItems
                    .Include(oi => oi.Item)
                    .Where(oi => ids.Contains(oi.ItemId) && oi.UserId == request.UserId)
                    .ToDictionaryAsync(oi => oi.ItemId, oi => oi.Item!);

                characterItems.HeadItem = GetItemWithChecks(request.HeadItemId, new[] { ItemType.HeadArmor }, itemsById);
                characterItems.CapeItem = GetItemWithChecks(request.CapeItemId, new[] { ItemType.Cape }, itemsById);
                characterItems.BodyItem = GetItemWithChecks(request.BodyItemId, new[] { ItemType.BodyArmor }, itemsById);
                characterItems.HandItem = GetItemWithChecks(request.HandItemId, new[] { ItemType.HandArmor }, itemsById);
                characterItems.LegItem = GetItemWithChecks(request.LegItemId, new[] { ItemType.LegArmor }, itemsById);
                characterItems.HorseHarnessItem = GetItemWithChecks(request.HorseHarnessItemId, new[] { ItemType.HorseHarness }, itemsById);
                characterItems.HorseItem = GetItemWithChecks(request.HorseItemId, new[] { ItemType.Horse }, itemsById);
                characterItems.Weapon1Item = GetItemWithChecks(request.Weapon1ItemId, WeaponTypes, itemsById);
                characterItems.Weapon2Item = GetItemWithChecks(request.Weapon2ItemId, WeaponTypes, itemsById);
                characterItems.Weapon3Item = GetItemWithChecks(request.Weapon3ItemId, WeaponTypes, itemsById);
                characterItems.Weapon4Item = GetItemWithChecks(request.Weapon4ItemId, WeaponTypes, itemsById);
                characterItems.AutoRepair = request.AutoRepair;
            }

            private Item? GetItemWithChecks(int? requestedItemId, IEnumerable<ItemType> expectedTypes,
                Dictionary<int, Item> itemsById)
            {
                if (requestedItemId == null)
                {
                    return null;
                }

                if (!itemsById.TryGetValue(requestedItemId.Value, out var item))
                {
                    // Not owned or not existing item.
                    throw new ItemNotOwnedException(requestedItemId.Value);
                }

                if (!expectedTypes.Contains(item.Type))
                {
                    throw new ItemBadTypeException(requestedItemId.Value, expectedTypes, item.Type);
                }

                return item;
            }

            private IEnumerable<int> BuildItemIdCollection(UpdateCharacterItemsCommand request)
            {
                var ids = new List<int>();
                if (request.HeadItemId != null)
                {
                    ids.Add(request.HeadItemId.Value);
                }

                if (request.CapeItemId != null)
                {
                    ids.Add(request.CapeItemId.Value);
                }

                if (request.BodyItemId != null)
                {
                    ids.Add(request.BodyItemId.Value);
                }

                if (request.HandItemId != null)
                {
                    ids.Add(request.HandItemId.Value);
                }

                if (request.LegItemId != null)
                {
                    ids.Add(request.LegItemId.Value);
                }

                if (request.HorseHarnessItemId != null)
                {
                    ids.Add(request.HorseHarnessItemId.Value);
                }

                if (request.HorseItemId != null)
                {
                    ids.Add(request.HorseItemId.Value);
                }

                if (request.Weapon1ItemId != null)
                {
                    ids.Add(request.Weapon1ItemId.Value);
                }

                if (request.Weapon2ItemId != null)
                {
                    ids.Add(request.Weapon2ItemId.Value);
                }

                if (request.Weapon3ItemId != null)
                {
                    ids.Add(request.Weapon3ItemId.Value);
                }

                if (request.Weapon4ItemId != null)
                {
                    ids.Add(request.Weapon4ItemId.Value);
                }

                return ids;
            }

            // Exceptions that will simplify error handling in this command.
            private class ItemNotOwnedException : Exception
            {
                public int ItemId { get; }

                public ItemNotOwnedException(int itemId) => ItemId = itemId;
            }

            private class ItemBadTypeException : Exception
            {
                public int ItemId { get; }
                public IEnumerable<ItemType> ExpectedTypes { get; }
                public ItemType ActualType { get; }

                public ItemBadTypeException(int itemId, IEnumerable<ItemType> expectedTypes, ItemType actualType)
                {
                    ItemId = itemId;
                    ExpectedTypes = expectedTypes;
                    ActualType = actualType;
                }
            }
        }
    }
}
