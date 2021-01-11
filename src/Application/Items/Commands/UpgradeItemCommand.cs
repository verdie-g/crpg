using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands
{
    public class UpgradeItemCommand : IMediatorRequest<ItemViewModel>
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<UpgradeItemCommand, ItemViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpgradeItemCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, IMapper mapper, Constants constants)
            {
                _db = db;
                _mapper = mapper;
                _constants = constants;
            }

            public async Task<Result<ItemViewModel>> Handle(UpgradeItemCommand req, CancellationToken cancellationToken)
            {
                var ownedItem = await _db.OwnedItems
                    .Include(oi => oi.User!)
                    .Include(oi => oi.Item!)
                    .Include(oi => oi.EquippedItems)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);
                if (ownedItem == null)
                {
                    return new Result<ItemViewModel>(CommonErrors.ItemNotOwned(req.ItemId));
                }

                if (ownedItem.Item!.Rank >= 3)
                {
                    return new Result<ItemViewModel>(CommonErrors.ItemMaxRankReached(req.ItemId, req.UserId, 3));
                }

                var upgradedItem = await _db.Items
                    .AsNoTracking()
                    .FirstAsync(i => i.BaseItemId == ownedItem.Item.BaseItemId && i.Rank == ownedItem.Item.Rank + 1, cancellationToken);
                if (ownedItem.Item.Rank < 0) // repair
                {
                    int repairCost = (int)MathHelper.ApplyPolynomialFunction(upgradedItem.Value, _constants.ItemRepairCostCoefs);
                    if (ownedItem.User!.Gold < repairCost)
                    {
                        return new Result<ItemViewModel>(CommonErrors.NotEnoughGold(repairCost, ownedItem.User!.Gold));
                    }

                    ownedItem.User!.Gold -= repairCost;
                }
                else // looming
                {
                    if (ownedItem.User!.HeirloomPoints == 0)
                    {
                        return new Result<ItemViewModel>(CommonErrors.NotEnoughHeirloomPoints(1, ownedItem.User.HeirloomPoints));
                    }

                    ownedItem.User!.HeirloomPoints -= 1;
                }

                var upgradedOwnedItem = new OwnedItem { User = ownedItem.User, ItemId = upgradedItem.Id };
                _db.OwnedItems.Add(upgradedOwnedItem);

                // If a character had the item equipped, replace the item by the upgraded one.
                foreach (var equippedItem in ownedItem.EquippedItems)
                {
                    equippedItem.ItemId = upgradedItem.Id;
                }

                _db.OwnedItems.Remove(ownedItem);
                await _db.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("User '{0}' upgraded item '{1}' to rank {2}", req.UserId, req.ItemId, upgradedItem.Rank);
                return new Result<ItemViewModel>(_mapper.Map<ItemViewModel>(upgradedItem));
            }
        }
    }
}
