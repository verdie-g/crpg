using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class SellItemCommand : IRequest
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<SellItemCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(SellItemCommand request, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(ui => ui.User).ThenInclude(u => u!.Characters)
                    .Include(ui => ui.Item)
                    .FirstOrDefaultAsync(ui => ui.UserId == request.UserId && ui.ItemId == request.ItemId, cancellationToken);

                if (userItem == null)
                {
                    throw new NotFoundException(nameof(UserItem), request.UserId, request.ItemId);
                }

                userItem.User!.Gold += (int)(userItem.Item!.Value * Constants.SellItemRatio);
                _db.UserItems.Remove(userItem);
                foreach (var character in userItem.User.Characters)
                {
                    UnsetItem(character, userItem.ItemId);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            private static void UnsetItem(Character character, int itemId)
            {
                if (character.HeadItemId == itemId)
                {
                    character.HeadItemId = null;
                }
                else if (character.CapeItemId == itemId)
                {
                    character.CapeItemId = null;
                }
                else if (character.BodyItemId == itemId)
                {
                    character.BodyItemId = null;
                }
                else if (character.HandItemId == itemId)
                {
                    character.HandItemId = null;
                }
                else if (character.LegItemId == itemId)
                {
                    character.LegItemId = null;
                }
                else if (character.HorseHarnessItemId == itemId)
                {
                    character.HorseHarnessItemId = null;
                }
                else if (character.HorseItemId == itemId)
                {
                    character.HorseItemId = null;
                }
                else if (character.Weapon1ItemId == itemId)
                {
                    character.Weapon1ItemId = null;
                }
                else if (character.Weapon2ItemId == itemId)
                {
                    character.Weapon2ItemId = null;
                }
                else if (character.Weapon3ItemId == itemId)
                {
                    character.Weapon3ItemId = null;
                }
                else if (character.Weapon4ItemId == itemId)
                {
                    character.Weapon4ItemId = null;
                }
            }
        }
    }
}