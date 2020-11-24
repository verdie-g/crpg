using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class UpgradeItemCommand : IMediatorRequest<ItemViewModel>
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<UpgradeItemCommand, ItemViewModel>
        {
            /// <summary>
            /// To repair an item for rank -1 to rank 0 it costs 7% of the rank 0 price.
            /// </summary>
            /// <remarks>This const should be synced with the Web UI.</remarks>
            private const float ItemRepairCost = 0.07f;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<ItemViewModel>> Handle(UpgradeItemCommand req, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(oi => oi.User!).ThenInclude(u => u.Characters)
                    .Include(oi => oi.Item!)
                    .FirstOrDefaultAsync(oi => oi.UserId == req.UserId && oi.ItemId == req.ItemId, cancellationToken);
                if (userItem == null)
                {
                    return new Result<ItemViewModel>(CommonErrors.ItemNotOwned(req.ItemId));
                }

                if (userItem.Item!.Rank >= 3)
                {
                    return new Result<ItemViewModel>(CommonErrors.ItemMaxRankReached(req.ItemId, req.UserId, 3));
                }

                var upgradedItem = await _db.Items
                    .AsNoTracking()
                    .FirstAsync(i => i.BaseItemId == userItem.Item.BaseItemId && i.Rank == userItem.Item.Rank + 1, cancellationToken);
                if (userItem.Item.Rank < 0) // repair
                {
                    int repairCost = (int)(ItemRepairCost * upgradedItem.Value);
                    if (userItem.User!.Gold < repairCost)
                    {
                        return new Result<ItemViewModel>(CommonErrors.NotEnoughGold(repairCost, userItem.User!.Gold));
                    }

                    userItem.User!.Gold -= repairCost;
                }
                else // looming
                {
                    if (userItem.User!.HeirloomPoints == 0)
                    {
                        return new Result<ItemViewModel>(CommonErrors.NotEnoughHeirloomPoints(1, userItem.User.HeirloomPoints));
                    }

                    userItem.User!.HeirloomPoints -= 1;
                }

                _db.UserItems.Remove(userItem);
                _db.UserItems.Add(new UserItem { User = userItem.User, ItemId = upgradedItem.Id });
                foreach (var character in userItem.User.Characters)
                {
                    CharacterHelper.ReplaceCharacterItem(character.Items, userItem.ItemId, upgradedItem.Id);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<ItemViewModel>(_mapper.Map<ItemViewModel>(upgradedItem));
            }
        }
    }
}
