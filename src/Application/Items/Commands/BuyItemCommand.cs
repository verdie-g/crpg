using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class BuyItemCommand : IMediatorRequest<ItemViewModel>
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<BuyItemCommand, ItemViewModel>
        {
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
                    return new Result<ItemViewModel>(CommonErrors.ItemNotFound(req.ItemId));
                }

                var user = await _db.Users
                    .Include(u => u.OwnedItems)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new Result<ItemViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.OwnedItems.Any(i => i.ItemId == req.ItemId))
                {
                    return new Result<ItemViewModel>(CommonErrors.ItemAlreadyOwned(req.ItemId));
                }

                if (user.Gold < item.Value)
                {
                    return new Result<ItemViewModel>(CommonErrors.NotEnoughGold(item.Value, user.Gold));
                }

                user.Gold -= item.Value;
                user.OwnedItems.Add(new UserItem { UserId = req.UserId, ItemId = req.ItemId });
                await _db.SaveChangesAsync(cancellationToken);
                return new Result<ItemViewModel>(_mapper.Map<ItemViewModel>(item));
            }
        }
    }
}
