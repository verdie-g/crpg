using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands
{
    public record SellItemCommand : IMediatorRequest
    {
        public int ItemId { get; init; }
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<SellItemCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<SellItemCommand>();

            private readonly ICrpgDbContext _db;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, Constants constants)
            {
                _db = db;
                _constants = constants;
            }

            public async Task<Result> Handle(SellItemCommand req, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(oi => oi.User)
                    .Include(oi => oi.Item)
                    .Include(oi => oi.EquippedItems)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);

                if (userItem == null)
                {
                    return new Result(CommonErrors.ItemNotOwned(req.ItemId));
                }

                userItem.User!.Gold += (int)MathHelper.ApplyPolynomialFunction(userItem.Item!.Value, _constants.ItemSellCostCoefs);
                _db.EquippedItems.RemoveRange(userItem.EquippedItems);
                _db.UserItems.Remove(userItem);

                await _db.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("User '{0}' sold item '{1}'", req.UserId, req.ItemId);
                return new Result();
            }
        }
    }
}
