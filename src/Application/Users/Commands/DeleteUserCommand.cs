using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Sdk.Abstractions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Commands
{
    /// <summary>
    /// Deletes all entities related to user except <see cref="Ban"/>s and reset user info.
    /// </summary>
    public class DeleteUserCommand : IMediatorRequest
    {
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<DeleteUserCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IEventService _events;

            public Handler(ICrpgDbContext db, IEventService events)
            {
                _db = db;
                _events = events;
            }

            public async Task<Result<object>> Handle(DeleteUserCommand req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.Characters)
                    .Include(u => u.OwnedItems)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new Result<object>(CommonErrors.UserNotFound(req.UserId));
                }

                string name = user.Name;

                UserHelper.SetDefaultValuesForUser(user);
                user.Name = string.Empty;
                user.AvatarSmall = new Uri("https://via.placeholder.com/32x32");
                user.AvatarMedium = new Uri("https://via.placeholder.com/64x64");
                user.AvatarFull = new Uri("https://via.placeholder.com/184x184");

                _db.UserItems.RemoveRange(user.OwnedItems);
                _db.Characters.RemoveRange(user.Characters);
                await _db.SaveChangesAsync(cancellationToken);
                _events.Raise(EventLevel.Info, $"{name} deleted its account ({user.Platform}#{user.PlatformUserId})", string.Empty, "user_deleted");
                return new Result<object>();
            }
        }
    }
}
