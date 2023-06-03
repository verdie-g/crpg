using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record UpgradeUserItemCommand : IMediatorRequest<UserItemViewModel>
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<UpgradeUserItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpgradeUserItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _constants = constants;
        }

        public async Task<Result<UserItemViewModel>> Handle(UpgradeUserItemCommand req, CancellationToken cancellationToken)
        {
            var userItem = await _db.UserItems
                .Include(ui => ui.User!)
                .Include(ui => ui.Item!)
                .FirstOrDefaultAsync(ui => ui.UserId == req.UserId && ui.Id == req.UserItemId, cancellationToken);
            if (userItem == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItem.Rank >= 3)
            {
                return new(CommonErrors.UserItemMaxRankReached(req.UserItemId, 3));
            }

            if (userItem.Rank < 0) // repair
            {
                int repairCost = (int)(userItem.Item!.Price
                                       * _constants.ItemRepairCostPerSecond
                                       * _constants.BrokenItemRepairPenaltySeconds);
                if (userItem.User!.Gold < repairCost)
                {
                    return new(CommonErrors.NotEnoughGold(repairCost, userItem.User!.Gold));
                }

                userItem.User!.Gold -= repairCost;

                _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(userItem.UserId, userItem.ItemId, repairCost, 0));
            }
            else // looming
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(9999, userItem.User!.HeirloomPoints));
#if false
                if (userItem.User!.HeirloomPoints == 0)
                {
                    return new(CommonErrors.NotEnoughHeirloomPoints(1, userItem.User.HeirloomPoints));
                }

                userItem.User!.HeirloomPoints -= 1;

                _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(userItem.UserId, userItem.BaseItemId, 0, 1));
#endif
            }

            userItem.Rank += 1;
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' upgraded user item '{1}' to rank {2}", req.UserId, req.UserItemId, userItem.Rank);
            return new(_mapper.Map<UserItemViewModel>(userItem));
        }
    }
}
