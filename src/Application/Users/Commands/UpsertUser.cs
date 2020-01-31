using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Common.Mappings;
using Trpg.Application.Steam;
using Trpg.Domain.Entities;

namespace Trpg.Application.Users.Commands
{
    public class UpsertUserCommand : IRequest<UserModelView>, IMapFrom<SteamPlayer>
    {
        public string SteamId { get; set; }
        public string UserName { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SteamPlayer, UpsertUserCommand>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(p => p.PersonaName));
        }

        public class Validator : AbstractValidator<UpsertUserCommand>
        {
            public Validator()
            {
                RuleFor(u => u.SteamId).Matches("^\\d{17}$");
                RuleFor(u => u.UserName).NotNull().NotEmpty();
                RuleFor(u => u.Avatar).NotNull();
                RuleFor(u => u.AvatarMedium).NotNull();
                RuleFor(u => u.AvatarFull).NotNull();
            }
        }

        public class Handler : IRequestHandler<UpsertUserCommand, UserModelView>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<UserModelView> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
            {
                var userEntity =
                    await _db.Users.FirstOrDefaultAsync(u => u.SteamId == request.SteamId, cancellationToken)
                    ?? new User {SteamId = request.SteamId};

                userEntity.UserName = request.UserName;
                userEntity.Avatar = request.Avatar;
                userEntity.AvatarMedium = request.AvatarMedium;
                userEntity.AvatarFull = request.AvatarFull;

                if (_db.Entry(userEntity).State == EntityState.Detached)
                {
                    userEntity.Role = Role.User;
                    _db.Users.Add(userEntity);
                }

                await _db.SaveChangesAsync(cancellationToken);

                return _mapper.Map<UserModelView>(userEntity);
            }
        }
    }
}