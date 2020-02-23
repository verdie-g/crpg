using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Steam;
using Crpg.Domain.Entities;

namespace Crpg.Application.Users.Commands
{
    public class UpsertUserCommand : IRequest<UserViewModel>, IMapFrom<SteamPlayer>
    {
        public long SteamId { get; set; }
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
                RuleFor(u => u.UserName).NotNull().NotEmpty();
                RuleFor(u => u.Avatar).NotNull();
                RuleFor(u => u.AvatarMedium).NotNull();
                RuleFor(u => u.AvatarFull).NotNull();
            }
        }

        public class Handler : IRequestHandler<UpsertUserCommand, UserViewModel>
        {
            private const int StartingMoney = 300;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<UserViewModel> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
            {
                var userEntity =
                    await _db.Users.FirstOrDefaultAsync(u => u.SteamId == request.SteamId, cancellationToken)
                    ?? new User {SteamId = request.SteamId};

                userEntity.UserName = request.UserName;
                userEntity.AvatarSmall = request.Avatar;
                userEntity.AvatarMedium = request.AvatarMedium;
                userEntity.AvatarFull = request.AvatarFull;

                if (_db.Entry(userEntity).State == EntityState.Detached)
                {
                    userEntity.Role = Role.User;
                    userEntity.Money = StartingMoney;
                    _db.Users.Add(userEntity);
                }

                await _db.SaveChangesAsync(cancellationToken);

                return _mapper.Map<UserViewModel>(userEntity);
            }
        }
    }
}