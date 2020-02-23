using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Commands
{
    public class SellItemCommand : IRequest
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<SellItemCommand>
        {
            private const float SellRatio = 0.66f;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
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

                userItem.User.Golds += (int) (userItem.Item.Value * SellRatio);
                _db.UserItems.Remove(userItem);
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}