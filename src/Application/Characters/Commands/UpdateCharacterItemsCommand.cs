using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters.Commands
{
    public class UpdateCharacterItemsCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public int? HeadItemId { get; set; }
        public int? BodyItemId { get; set; }
        public int? LegsItemId { get; set; }
        public int? GlovesItemId { get; set; }
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

            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(UpdateCharacterItemsCommand request,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.HeadItem)
                    .Include(c => c.BodyItem)
                    .Include(c => c.LegsItem)
                    .Include(c => c.GlovesItem)
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
                character.BodyItem = GetItemWithChecks(request.BodyItemId, new[] { ItemType.BodyArmor }, itemsById);
                character.LegsItem = GetItemWithChecks(request.LegsItemId, new[] { ItemType.LegArmor }, itemsById);
                character.GlovesItem = GetItemWithChecks(request.GlovesItemId, new[] { ItemType.HandArmor }, itemsById);
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
                    throw new BadRequestException($"Unexpected item");
                }

                return item;
            }

            private IEnumerable<int> BuildItemIdCollection(UpdateCharacterItemsCommand request)
            {
                var ids = new List<int>();
                if (request.HeadItemId != null)
                    ids.Add(request.HeadItemId.Value);
                if (request.BodyItemId != null)
                    ids.Add(request.BodyItemId.Value);
                if (request.LegsItemId != null)
                    ids.Add(request.LegsItemId.Value);
                if (request.GlovesItemId != null)
                    ids.Add(request.GlovesItemId.Value);
                if (request.Weapon1ItemId != null)
                    ids.Add(request.Weapon1ItemId.Value);
                if (request.Weapon2ItemId != null)
                    ids.Add(request.Weapon2ItemId.Value);
                if (request.Weapon3ItemId != null)
                    ids.Add(request.Weapon3ItemId.Value);
                if (request.Weapon4ItemId != null)
                    ids.Add(request.Weapon4ItemId.Value);

                return ids;
            }
        }
    }
}