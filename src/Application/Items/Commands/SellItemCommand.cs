using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class SellItemCommand : IMediatorRequest
{
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<SellItemCommand>
        {
            private const float SellItemRatio = 0.66f;

            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(SellItemCommand req, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(oi => oi.User).ThenInclude(u => u!.Characters)
                    .Include(oi => oi.Item)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);

                if (userItem == null)
                {
                    return new Result(CommonErrors.ItemNotOwned(req.ItemId));
                }

                userItem.User!.Gold += (int)(userItem.Item!.Value * SellItemRatio);
                _db.UserItems.Remove(userItem);
                foreach (var character in userItem.User.Characters)
                {
                    UnsetItem(character.Items, userItem.ItemId);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }

            private static void UnsetItem(CharacterItems items, int itemId)
            {
                if (items.HeadItemId == itemId)
                {
                    items.HeadItemId = null;
                }
                else if (items.CapeItemId == itemId)
                {
                    items.CapeItemId = null;
                }
                else if (items.BodyItemId == itemId)
                {
                    items.BodyItemId = null;
                }
                else if (items.HandItemId == itemId)
                {
                    items.HandItemId = null;
                }
                else if (items.LegItemId == itemId)
                {
                    items.LegItemId = null;
                }
                else if (items.HorseHarnessItemId == itemId)
                {
                    items.HorseHarnessItemId = null;
                }
                else if (items.HorseItemId == itemId)
                {
                    items.HorseItemId = null;
                }
                else if (items.Weapon1ItemId == itemId)
                {
                    items.Weapon1ItemId = null;
                }
                else if (items.Weapon2ItemId == itemId)
                {
                    items.Weapon2ItemId = null;
                }
                else if (items.Weapon3ItemId == itemId)
                {
                    items.Weapon3ItemId = null;
                }
                else if (items.Weapon4ItemId == itemId)
                {
                    items.Weapon4ItemId = null;
                }
            }
        }
    }
}
