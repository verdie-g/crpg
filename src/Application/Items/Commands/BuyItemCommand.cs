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

public record BuyItemCommand : IMediatorRequest<ItemViewModel>
{
    public int ItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<BuyItemCommand, ItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuyItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<ItemViewModel>> Handle(BuyItemCommand req, CancellationToken cancellationToken)
        {
            var item = await _db.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            if (item.Rank != 0)
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

            if (user.Items.Any(i => i.ItemId == req.ItemId))
            {
                return new(CommonErrors.ItemAlreadyOwned(req.ItemId));
            }

            if (user.Gold < item.Price)
            {
                return new(CommonErrors.NotEnoughGold(item.Price, user.Gold));
            }

            user.Gold -= item.Price;
            user.Items.Add(new UserItem { UserId = req.UserId, ItemId = req.ItemId });
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' bought item '{1}'", req.UserId, req.ItemId);
            return new(_mapper.Map<ItemViewModel>(item));
        }
    }
}
