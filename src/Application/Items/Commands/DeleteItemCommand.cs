using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Application.Items.Commands
{
    public class DeleteItemCommand : IRequest
    {
        public int ItemId { get; set; }

        public class Handler : IRequestHandler<DeleteItemCommand>
        {
            private readonly ITrpgDbContext _db;

            public Handler(ITrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
            {
                var itemDb = await _db.Items.FindAsync(request.ItemId);
                if (itemDb == null)
                {
                    throw new NotFoundException(nameof(Items), request.ItemId);
                }

                _db.Items.Remove(itemDb);
                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
