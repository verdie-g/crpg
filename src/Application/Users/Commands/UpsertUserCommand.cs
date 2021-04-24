using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Steam;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Commands
{
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
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEventService _events;
            private readonly IUserService _userService;

            public Handler(ICrpgDbContext db, IMapper mapper, IEventService events, IUserService userService)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
                _userService = userService;
            }

            public async Task<Result<UserViewModel>> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
            {
                var user =
                    await _db.Users.FirstOrDefaultAsync(u => u.PlatformUserId == request.PlatformUserId, cancellationToken)
                    ?? new User { Platform = Platform.Steam, PlatformUserId = request.PlatformUserId };

                user.Name = request.Name;
                user.AvatarSmall = request.Avatar;
                user.AvatarMedium = request.AvatarMedium;
                user.AvatarFull = request.AvatarFull;
                // If the user has deleted its account, recreate it.
                user.DeletedAt = null;

                if (_db.Entry(user).State == EntityState.Detached)
                {
                    _userService.SetDefaultValuesForUser(user);
                    _db.Users.Add(user);
                    _events.Raise(EventLevel.Info, $"{request.Name} joined ({user.Platform}#{user.PlatformUserId})", string.Empty, "user_created");
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new(_mapper.Map<UserViewModel>(user));
            }
        }
    }
}
