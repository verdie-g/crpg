using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands
{
    public class BuyItemCommand : IRequest<ItemViewModel>
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<BuyItemCommand, ItemViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ItemViewModel> Handle(BuyItemCommand request, CancellationToken cancellationToken)
            {
                var item = await _db.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
                if (item == null)
                {
                    throw new NotFoundException(nameof(Item), request.ItemId);
                }

                var user = await _db.Users
                    .Include(u => u.UserItems)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                if (user.UserItems.Any(i => i.ItemId == request.ItemId))
                {
                    throw new BadRequestException("User already owns this item");
                }

                if (user.Gold < item.Value)
                {
                    throw new BadRequestException("User doesn't have enough gold");
                }

                user.Gold -= item.Value;
                user.UserItems.Add(new UserItem { UserId = request.UserId, ItemId = request.ItemId });
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<ItemViewModel>(item);
            }
        }
    }
}