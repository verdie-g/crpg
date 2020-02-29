using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterItemsCommand : IRequest<CharacterViewModel>
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

        public class Handler : IRequestHandler<UpdateCharacterItemsCommand, CharacterViewModel>
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

            public async Task<CharacterViewModel> Handle(UpdateCharacterItemsCommand request,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.HeadItem)
                    .Include(c => c.CapeItem)
                    .Include(c => c.BodyItem)
                    .Include(c => c.HandItem)
                    .Include(c => c.LegItem)
                    .Include(c => c.HorseHarnessItem)
                    .Include(c => c.HorseItem)
                    .Include(c => c.Weapon1Item)
                    .Include(c => c.Weapon2Item)
                    .Include(c => c.Weapon3Item)
                    .Include(c => c.Weapon4Item)
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId, request.UserId);
                }

                await UpdateCharacterItems(request, character);
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }

            private async Task UpdateCharacterItems(UpdateCharacterItemsCommand request, Character character)
            {
                var ids = BuildItemIdCollection(request);
                var itemsById = await _db.UserItems
                    .Include(ui => ui.Item)
                    .Where(ui => ids.Contains(ui.ItemId) && ui.UserId == request.UserId)
                    .ToDictionaryAsync(ui => ui.ItemId, ui => ui.Item);

                character.HeadItem = GetItemWithChecks(request.HeadItemId, new[] { ItemType.HeadArmor }, itemsById);
                character.CapeItem = GetItemWithChecks(request.CapeItemId, new[] { ItemType.Cape }, itemsById);
                character.BodyItem = GetItemWithChecks(request.BodyItemId, new[] { ItemType.BodyArmor }, itemsById);
                character.HandItem = GetItemWithChecks(request.HandItemId, new[] { ItemType.HandArmor }, itemsById);
                character.LegItem = GetItemWithChecks(request.LegItemId, new[] { ItemType.LegArmor }, itemsById);
                character.HorseHarnessItem = GetItemWithChecks(request.HorseHarnessItemId, new[] { ItemType.HorseHarness }, itemsById);
                character.HorseItem = GetItemWithChecks(request.HorseItemId, new[] { ItemType.Horse }, itemsById);
                character.Weapon1Item = GetItemWithChecks(request.Weapon1ItemId, WeaponTypes, itemsById);
                character.Weapon2Item = GetItemWithChecks(request.Weapon2ItemId, WeaponTypes, itemsById);
                character.Weapon3Item = GetItemWithChecks(request.Weapon3ItemId, WeaponTypes, itemsById);
                character.Weapon4Item = GetItemWithChecks(request.Weapon4ItemId, WeaponTypes, itemsById);
            }

            private Item GetItemWithChecks(int? id, IEnumerable<ItemType> expectedTypes,
                Dictionary<int, Item> itemsById)
            {
                if (id == null)
                {
                    return null;
                }

                if (!itemsById.TryGetValue(id.Value, out var item) || !expectedTypes.Contains(item.Type))
                {
                    throw new BadRequestException("Unexpected item");
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
        }
    }
}