using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
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

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<Result<UserItemViewModel>> Handle(UpgradeUserItemCommand req, CancellationToken cancellationToken)
        {
            var userItem = await _db.UserItems
                .Include(ui => ui.User!)
                .Include(ui => ui.Item!)
                .FirstOrDefaultAsync(ui => ui.UserId == req.UserId && ui.Id == req.UserItemId, cancellationToken);

            var user = await _db.Users
                .Include(u => u.Items)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var userUserItems = await _db.UserItems
                .Where(ui => user.Items.Contains(ui))
                .Include(ui => ui.Item)
                .ToListAsync();

            if (userItem == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItem.Item!.Type == ItemType.Banner)
            {
                return new(CommonErrors.ItemNotUpgradable(userItem.ItemId));
            }

            if (userUserItems.Any(ui => ui.Item!.BaseId == userItem.Item.BaseId && ui.Item!.Rank == userItem.Item.Rank + 1))
            {
                return new(CommonErrors.ItemAlreadyOwned($"{userItem.ItemId} +1 version"));
            }

            if (userItem.Item!.Rank == 3)
            {
                return new(CommonErrors.UserItemMaxRankReached(userItem.Id, 3));
            }

            if (user.HeirloomPoints < 1)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(1, user.HeirloomPoints));
            }

            Item? heirloomedItem = await _db.Items
                .Where(i => i.BaseId == userItem.Item!.BaseId && i.Rank == userItem.Item!.Rank + 1)
                .FirstOrDefaultAsync();

            if (heirloomedItem == null)
            {
                return new(CommonErrors.ItemUpgradedVersionNotFound(userItem.Item.Id));
            }

            var heirloomedUserItem = new UserItem
            {
                UserId = req.UserId,
                Item = heirloomedItem,
                IsBroken = userItem.IsBroken,
            };

            user.Items.Remove(userItem);
            user.Items.Add(heirloomedUserItem);
            user.HeirloomPoints -= 1;
            _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(user.Id, userItem.ItemId, 1));
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' has upgraded item '{1}'", req.UserId, req.UserItemId);
            return new(_mapper.Map<UserItemViewModel>(heirloomedUserItem));
        }
    }
}
