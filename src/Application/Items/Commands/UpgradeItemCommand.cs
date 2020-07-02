using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class UpgradeItemCommand : IRequest<ItemViewModel>
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<UpgradeItemCommand, ItemViewModel>
        {
            /// <summary>
            /// To repair an item for rank -1 to rank 0 it costs 7% of the rank 0 price.
            /// </summary>
            private const float ItemRepairCost = 0.07f;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ItemViewModel> Handle(UpgradeItemCommand request, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(ui => ui.User!).ThenInclude(u => u.Characters)
                    .Include(ui => ui.Item!)
                    .FirstOrDefaultAsync(ui => ui.UserId == request.UserId && ui.ItemId == request.ItemId, cancellationToken);
                if (userItem == null)
                {
                    throw new NotFoundException(nameof(UserItem), request.UserId, request.ItemId);
                }

                CheckItemRank(userItem.Item!.Rank);

                var upgradedItem = await _db.Items
                    .AsNoTracking()
                    .FirstAsync(i => i.BaseItemId == userItem.Item.BaseItemId && i.Rank == userItem.Item.Rank + 1, cancellationToken);
                if (userItem.Item.Rank < 0) // repair
                {
                    int repairCost = (int)(ItemRepairCost * upgradedItem.Value);
                    if (userItem.User!.Gold < repairCost)
                    {
                        throw new BadRequestException("Not enough gold");
                    }

                    userItem.User!.Gold -= repairCost;
                }
                else // looming
                {
                    if (userItem.User!.HeirloomPoints == 0)
                    {
                        throw new BadRequestException("No heirloom points");
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
                return _mapper.Map<ItemViewModel>(upgradedItem);
            }

            private static void CheckItemRank(int rank)
            {
                if (rank >= 3)
                {
                    throw new BadRequestException("Item is already at its maximal rank");
                }

                if (rank < -3) // inconsistency in database
                {
                    throw new Exception($"Unknown rank {rank}");
                }
            }
        }
    }
}