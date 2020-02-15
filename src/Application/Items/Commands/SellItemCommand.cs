using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Items.Commands
{
    public class SellItemCommand : IRequest
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<SellItemCommand>
        {
            private const float SellRatio = 0.66f;

            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(SellItemCommand request, CancellationToken cancellationToken)
            {
                var userItem = await _db.UserItems
                    .Include(ui => ui.User)
                    .Include(ui => ui.Item)
                    .FirstOrDefaultAsync(ui => ui.UserId == request.UserId && ui.ItemId == request.ItemId, cancellationToken);

                if (userItem == null)
                {
                    throw new NotFoundException(nameof(UserItem), request.UserId, request.ItemId);
                }

                userItem.User.Money += (int) (userItem.Item.Price * SellRatio);
                _db.UserItems.Remove(userItem);
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}