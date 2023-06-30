using AutoMapper;
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

public record ReforgeUpgradedUserItemCommand : IMediatorRequest<UserItemViewModel>
{
    private static readonly Dictionary<int, int> ReforgePriceForRank = new()
    {
        [1] = 40000,
        [2] = 90000,
        [3] = 150000,
    };

    public int UserItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<ReforgeUpgradedUserItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ReforgeUpgradedUserItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<Result<UserItemViewModel>> Handle(ReforgeUpgradedUserItemCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsSplitQuery()
                .Include(u => u.Items).ThenInclude(ui => ui.Item)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var userItemToReforge = user.Items
                .FirstOrDefault(ui => ui.Id == req.UserItemId);

            if (userItemToReforge == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItemToReforge.IsBroken)
            {
                return new(CommonErrors.ItemBroken(userItemToReforge.ItemId));
            }

            if (userItemToReforge.Item!.Type == ItemType.Banner)
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            if (userItemToReforge.Item.Rank == 0)
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            if (!ReforgePriceForRank.TryGetValue(userItemToReforge.Item.Rank, out int price))
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            if (user.Gold < price)
            {
                return new(CommonErrors.NotEnoughGold(price, user.Gold));
            }

            Item? baseItem = await _db.Items
                .FirstOrDefaultAsync(
                    i => i.BaseId == userItemToReforge.Item!.BaseId && i.Rank == 0,
                    cancellationToken);

            if (baseItem == null)
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            if (user.Items.Any(ui => ui.Item!.Id == baseItem.Id))
            {
                return new(CommonErrors.ItemAlreadyOwned(baseItem.Id));
            }

            _db.ActivityLogs.Add(_activityLogService.CreateItemReforgedLog(user.Id, userItemToReforge.ItemId, userItemToReforge.Item.Rank));
            user.HeirloomPoints += userItemToReforge.Item.Rank;
            user.Gold -= price;
            userItemToReforge.Item = baseItem;

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' has Reforged item '{1}'", req.UserId, req.UserItemId);
            return new(_mapper.Map<UserItemViewModel>(userItemToReforge));
        }
    }
}
