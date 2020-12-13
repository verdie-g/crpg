using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class SellItemCommand : IMediatorRequest
{
        public int ItemId { get; set; }
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<SellItemCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, Constants constants)
            {
                _db = db;
                _constants = constants;
            }

            public async Task<Result> Handle(SellItemCommand req, CancellationToken cancellationToken)
            {
                var ownedItem = await _db.OwnedItems
                    .Include(oi => oi.User)
                    .Include(oi => oi.Item)
                    .Include(oi => oi.EquippedItems)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);

                if (ownedItem == null)
                {
                    return new Result(CommonErrors.ItemNotOwned(req.ItemId));
                }

                ownedItem.User!.Gold += (int)MathHelper.ApplyPolynomialFunction(ownedItem.Item!.Value, _constants.ItemSellCostCoefs);
                _db.EquippedItems.RemoveRange(ownedItem.EquippedItems);
                _db.OwnedItems.Remove(ownedItem);

                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }
        }
    }
}
