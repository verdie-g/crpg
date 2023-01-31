using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Steam;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpsertUserCommand : IMediatorRequest<UserViewModel>, IMapFrom<SteamPlayer>
{
    public string PlatformUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Uri Avatar { get; set; } = default!;
    public Uri AvatarMedium { get; set; } = default!;
    public Uri AvatarFull { get; set; } = default!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SteamPlayer, UpsertUserCommand>()
            .ForMember(u => u.PlatformUserId, opt => opt.MapFrom(p => p.SteamId))
            .ForMember(u => u.Name, opt => opt.MapFrom(p => p.PersonaName));
    }

    public class Validator : AbstractValidator<UpsertUserCommand>
    {
        public Validator()
        {
            RuleFor(u => u.Name).NotNull().NotEmpty();
            RuleFor(u => u.Avatar).NotNull();
            RuleFor(u => u.AvatarMedium).NotNull();
            RuleFor(u => u.AvatarFull).NotNull();
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
                .FirstOrDefaultAsync(u => u.PlatformUserId == request.PlatformUserId, cancellationToken);

            if (user == null)
            {
                user = new User { Platform = Platform.Steam, PlatformUserId = request.PlatformUserId };
                _userService.SetDefaultValuesForUser(user);
                _db.Users.Add(user);
                newUser = true;
            }

            string oldName = user.Name;

            user.Name = request.Name;
            user.AvatarSmall = request.Avatar;
            user.AvatarMedium = request.AvatarMedium;
            user.AvatarFull = request.AvatarFull;
            // If the user has deleted its account, recreate it.
            user.DeletedAt = null;

            if (user.Name != oldName)
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
