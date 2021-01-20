using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Commands
{
    public class CreateClanCommand : IMediatorRequest<ClanViewModel>
    {
        public int UserId { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public class Validator : AbstractValidator<CreateClanCommand>
        {
            private const int MinimumClanTagLength = 2;
            private const int MaximumClanTagLength = 8;

            private const int MinimumClanNameLength = 2;
            private const int MaximumClanNameLength = 32;

            public Validator()
            {
                RuleFor(c => c.Tag)
                    .MinimumLength(MinimumClanTagLength)
                    .MaximumLength(MaximumClanTagLength);

                RuleFor(c => c.Name)
                    .MinimumLength(MinimumClanNameLength)
                    .MaximumLength(MaximumClanNameLength);
            }
        }

        internal class Handler : IMediatorRequestHandler<CreateClanCommand, ClanViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<ClanViewModel>> Handle(CreateClanCommand req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new Result<ClanViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.ClanMembership != null)
                {
                    return new Result<ClanViewModel>(CommonErrors.UserAlreadyInAClan(req.UserId));
                }

                var clan = await _db.Clans
                    .FirstOrDefaultAsync(c => c.Tag == req.Tag || c.Name == req.Name, cancellationToken);
                if (clan != null)
                {
                    return clan.Tag == req.Tag
                        ? new Result<ClanViewModel>(CommonErrors.ClanTagAlreadyUsed(clan.Tag))
                        : new Result<ClanViewModel>(CommonErrors.ClanNameAlreadyUsed(clan.Name));
                }

                clan = new Clan
                {
                    Tag = req.Tag,
                    Name = req.Name,
                    Members =
                    {
                        new ClanMember
                        {
                            User = user,
                            Role = ClanMemberRole.Leader,
                        },
                    },
                };

                _db.Clans.Add(clan);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result<ClanViewModel>(_mapper.Map<ClanViewModel>(clan));
            }
        }
    }
}
