using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record BuyItemCommand : IMediatorRequest<UserItemViewModel>
{
    public string ItemId { get; init; } = string.Empty;
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<BuyItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuyItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<UserItemViewModel>> Handle(BuyItemCommand req, CancellationToken cancellationToken)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            if (item.Type == ItemType.Banner)
            {
                return new(CommonErrors.ItemNotBuyable(req.ItemId));
            }

            var user = await _db.Users
                .Include(u => u.Items)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.Items.Any(ui => ui.BaseItemId == req.ItemId && ui.Rank == 0))
            {
                return new(CommonErrors.ItemAlreadyOwned(req.ItemId));
            }

            if (user.Gold < item.Price)
            {
                return new(CommonErrors.NotEnoughGold(item.Price, user.Gold));
            }

            user.Gold -= item.Price;
            var userItem = new UserItem
            {
                UserId = req.UserId,
                Rank = 0,
                BaseItem = item,
            };
            user.Items.Add(userItem);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' bought item '{1}'", req.UserId, req.ItemId);
            return new(_mapper.Map<UserItemViewModel>(userItem));
        }
    }
}
