using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Steam;
using Crpg.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Commands
{
    public class UpsertUserCommand : IRequest<UserViewModel>, IMapFrom<SteamPlayer>
    {
        public ulong SteamId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Uri Avatar { get; set; } = default!;
        public Uri AvatarMedium { get; set; } = default!;
        public Uri AvatarFull { get; set; } = default!;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SteamPlayer, UpsertUserCommand>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(p => p.PersonaName));
        }

        public class Validator : AbstractValidator<UpsertUserCommand>
        {
            public Validator()
            {
                RuleFor(u => u.UserName).NotNull().NotEmpty();
                RuleFor(u => u.Avatar).NotNull();
                RuleFor(u => u.AvatarMedium).NotNull();
                RuleFor(u => u.AvatarFull).NotNull();
            }
        }

        public class Handler : IRequestHandler<UpsertUserCommand, UserViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEventRaiser _events;

            public Handler(ICrpgDbContext db, IMapper mapper, IEventRaiser events)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
            }

            public async Task<UserViewModel> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
            {
                var user =
                    await _db.Users.FirstOrDefaultAsync(u => u.SteamId == request.SteamId, cancellationToken)
                    ?? new User { SteamId = request.SteamId };

                user.UserName = request.UserName;
                user.AvatarSmall = request.Avatar;
                user.AvatarMedium = request.AvatarMedium;
                user.AvatarFull = request.AvatarFull;

                if (_db.Entry(user).State == EntityState.Detached)
                {
                    UserHelper.SetDefaultValuesForUser(user);
                    _db.Users.Add(user);
                    _events.Raise(EventLevel.Info, $"{request.UserName} joined ({request.SteamId})", string.Empty, "new_user");
                }

                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<UserViewModel>(user);
            }
        }
    }
}