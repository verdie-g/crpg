using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Commands
{
    /// <summary>
    /// Deletes all entities related to user except <see cref="Ban"/>s and reset user info.
    /// </summary>
    public class DeleteUserCommand : IRequest<Unit>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<DeleteUserCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IEventRaiser _events;

            public Handler(ICrpgDbContext db, IEventRaiser events)
            {
                _db = db;
                _events = events;
            }

            public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.Characters)
                    .Include(u => u.UserItems)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(user), request.UserId);
                }

                string userName = user.UserName;
                long steamId = user.SteamId;

                UserHelper.SetDefaultValuesForUser(user);
                user.UserName = string.Empty;
                user.AvatarSmall = new Uri("https://via.placeholder.com/32x32");
                user.AvatarMedium = new Uri("https://via.placeholder.com/64x64");
                user.AvatarFull = new Uri("https://via.placeholder.com/184x184");

                _db.UserItems.RemoveRange(user.UserItems);
                _db.Characters.RemoveRange(user.Characters);
                await _db.SaveChangesAsync(cancellationToken);
                _events.Raise(EventLevel.Info, $"{userName} deleted its account ({steamId})", string.Empty, "user_deleted");
                return Unit.Value;
            }
        }
    }
}