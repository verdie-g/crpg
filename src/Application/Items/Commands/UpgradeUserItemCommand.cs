using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
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
            var user = await _db.Users
                .Include(ui => ui.Items!)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (userItem == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItem.Item == null)
            {
                // to do
                // why would baseitem be null????
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItem.BrokenState < 0)
            {
                return new(CommonErrors.ItemBroken(userItem.ItemId));
            }

            if (userItem.Item.Rank >= 3)
            {
                return new(CommonErrors.UserItemMaxRankReached(req.UserItemId, 3));
            }

            if (userItem.User!.HeirloomPoints == 0)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(1, userItem.User.HeirloomPoints));
            }

            user.Items.Remove(userItem);

            string itemIdToAdd = userItem.ItemId[..^1] + (userItem.Item.Rank + 1).ToString();

            var heirloomedItem = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == itemIdToAdd, cancellationToken);
            var heirloomedUserItem = new UserItem
            {
                UserId = req.UserId,
                Item = heirloomedItem,
            };

            user.Items.Add(userItem);

            userItem.User!.HeirloomPoints -= 1;

            _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(userItem.UserId, userItem.ItemId, 0, 1));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' upgraded user item '{1}' to rank {2}", req.UserId, req.UserItemId, userItem.Item.Rank);
            return new(_mapper.Map<UserItemViewModel>(userItem));
        }
    }
}
