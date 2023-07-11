using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpsertUserCommand : IMediatorRequest<UserViewModel>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Uri Avatar { get; init; } = default!;

    public class Validator : AbstractValidator<UpsertUserCommand>
    {
        public Validator()
        {
            RuleFor(u => u.Platform).IsInEnum();
            RuleFor(u => u.Name).NotNull().NotEmpty();
            RuleFor(u => u.Avatar).NotNull();
        }
    }

    internal class Handler : IMediatorRequestHandler<UpsertUserCommand, UserViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpsertUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IUserService userService, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<UserViewModel>> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
        {
            bool newUser = false;
            var user = await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Platform == request.Platform && u.PlatformUserId == request.PlatformUserId, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Platform = request.Platform,
                    PlatformUserId = request.PlatformUserId,
                    Region = Region.Eu, // Hardcode region. It will be overriden by the real region when the user requests a token.
                };
                _userService.SetDefaultValuesForUser(user);
                _db.Users.Add(user);
                newUser = true;
            }

            string oldName = user.Name;

            user.Name = request.Name;
            user.Avatar = request.Avatar;
            // If the user has deleted its account, recreate it.
            user.DeletedAt = null;

            if (!newUser && user.Name != oldName)
            {
                _db.ActivityLogs.Add(_activityLogService.CreateUserRenamedLog(user.Id, oldName, user.Name));
            }

            await _db.SaveChangesAsync(cancellationToken);

            if (newUser)
            {
                Logger.LogInformation("{0} joined ({1}#{2})", request.Name, user.Platform, user.PlatformUserId);
                _db.ActivityLogs.Add(_activityLogService.CreateUserCreatedLog(user.Id));
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new(_mapper.Map<UserViewModel>(user));
        }
    }
}
