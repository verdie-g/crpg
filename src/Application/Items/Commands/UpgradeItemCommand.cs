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

namespace Crpg.Application.Items.Commands;

public record UpgradeItemCommand : IMediatorRequest<ItemViewModel>
{
    public int ItemId { get; init; }
    public int UserId { get; init; }

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
            var userItem = await _db.UserItems
                .Include(oi => oi.User!)
                .Include(oi => oi.Item!)
                .Include(oi => oi.EquippedItems)
                .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);
            if (userItem == null)
            {
                return new(CommonErrors.ItemNotOwned(req.ItemId));
            }

            if (userItem.Item!.Rank >= 3)
            {
                return new(CommonErrors.ItemMaxRankReached(req.ItemId, req.UserId, 3));
            }

            var upgradedItem = await _db.Items
                .AsNoTracking()
                .FirstAsync(i => i.BaseItemId == userItem.Item.BaseItemId && i.Rank == userItem.Item.Rank + 1, cancellationToken);
            if (userItem.Item.Rank < 0) // repair
            {
                int repairCost = (int)MathHelper.ApplyPolynomialFunction(upgradedItem.Value, _constants.ItemRepairCostCoefs);
                if (userItem.User!.Gold < repairCost)
                {
                    return new(CommonErrors.NotEnoughGold(repairCost, userItem.User!.Gold));
                }

                userItem.User!.Gold -= repairCost;
            }
            else // looming
            {
                if (userItem.User!.HeirloomPoints == 0)
                {
                    return new(CommonErrors.NotEnoughHeirloomPoints(1, userItem.User.HeirloomPoints));
                }

                userItem.User!.HeirloomPoints -= 1;
            }

            UserItem upgradedUserItem = new() { User = userItem.User, ItemId = upgradedItem.Id };
            _db.UserItems.Add(upgradedUserItem);

            // If a character had the item equipped, replace the item by the upgraded one.
            foreach (var equippedItem in userItem.EquippedItems)
            {
                equippedItem.ItemId = upgradedItem.Id;
            }

            _db.UserItems.Remove(userItem);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' upgraded item '{1}' to rank {2}", req.UserId, req.ItemId, upgradedItem.Rank);
            return new(_mapper.Map<ItemViewModel>(upgradedItem));
        }
    }
}
