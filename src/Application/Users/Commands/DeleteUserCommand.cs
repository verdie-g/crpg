using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

/// <summary>
/// Deletes all entities related to user except <see cref="Ban"/>s and reset user info.
/// </summary>
public record DeleteUserCommand : IMediatorRequest
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<DeleteUserCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IDateTimeOffset _dateTimeOffset;
        private readonly IUserService _userService;

        public Handler(ICrpgDbContext db, IDateTimeOffset dateTimeOffset, IUserService userService)
        {
            _db = db;
            _dateTimeOffset = dateTimeOffset;
            _userService = userService;
        }

        public async Task<Result> Handle(DeleteUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Characters)
                .Include(u => u.Items)
                .Include(u => u.Hero!).ThenInclude(h => h.Items)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new Result(CommonErrors.UserNotFound(req.UserId));
            }

            string name = user.Name;

            _userService.SetDefaultValuesForUser(user);
            user.Name = string.Empty;
            user.AvatarSmall = new Uri("https://via.placeholder.com/32x32");
            user.AvatarMedium = new Uri("https://via.placeholder.com/64x64");
            user.AvatarFull = new Uri("https://via.placeholder.com/184x184");
            user.DeletedAt = _dateTimeOffset.Now; // Deleted users are just marked with a DeletedAt != null

            _db.UserItems.RemoveRange(user.Items);
            _db.Characters.RemoveRange(user.Characters);
            _db.HeroItems.RemoveRange(user.Hero!.Items);
            _db.Heroes.Remove(user.Hero);
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("{0} left ({1}#{2})", name, user.Platform, user.PlatformUserId);
            return Result.NoErrors;
        }
    }
}
