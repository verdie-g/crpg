using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;

namespace Crpg.Application.Items.Commands
{
    public class DeleteItemCommand : IRequest
    {
        public int ItemId { get; set; }

        public class Handler : IRequestHandler<DeleteItemCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
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
