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

public record HeirloomUserItemCommand : IMediatorRequest<UserItemViewModel>
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<HeirloomUserItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<HeirloomUserItemCommand>();

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

        public async Task<Result<UserItemViewModel>> Handle(HeirloomUserItemCommand req, CancellationToken cancellationToken)
        {
            var userItem = await _db.UserItems
                .Include(ui => ui.User!)
                .Include(ui => ui.BaseItem!)
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

            if (userItem.BaseItem == null)
            {
                // to do
                // why would baseitem be null????
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItem.BaseItem.HeirloomLevel >= 3)
            {
                return new(CommonErrors.UserItemMaxHeirloomLevelReached(req.UserItemId, 3));
            }

            if (userItem.User!.HeirloomPoints == 0)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(1, userItem.User.HeirloomPoints));
            }

            user.Items.Remove(userItem);

            string itemIdToAdd = userItem.BaseItemId[..^1] + (userItem.BaseItem.HeirloomLevel + 1).ToString();

            var heirloomedItem = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == itemIdToAdd, cancellationToken);
            var heirloomedUserItem = new UserItem
            {
                UserId = req.UserId,
                Rank = 0,
                BaseItem = heirloomedItem,
            };

            user.Items.Add(userItem);

            userItem.User!.HeirloomPoints -= 1;

            // to do _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(userItem.UserId, userItem.BaseItemId, 0, 1)); 

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' upgraded user item '{1}' to rank {2}", req.UserId, req.UserItemId, userItem.Rank);
            return new(_mapper.Map<UserItemViewModel>(userItem));
        }
    }
}
