using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
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
                    .Include(ui => ui.User)
                    .Include(ui => ui.Item)
                    .Include(ui => ui.EquippedItems)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);

                if (userItem == null)
                {
                    return new Result(CommonErrors.ItemNotOwned(req.ItemId));
                }

                userItem.User!.Gold += (int)(userItem.Item!.Value * SellItemRatio);
                _db.EquippedItems.RemoveRange(userItem.EquippedItems);
                _db.UserItems.Remove(userItem);

                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }
        }
    }
}
