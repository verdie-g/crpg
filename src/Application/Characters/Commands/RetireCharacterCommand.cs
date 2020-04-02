using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class RetireCharacterCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }

        public class Handler : IRequestHandler<RetireCharacterCommand, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(RetireCharacterCommand request, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId, cancellationToken);
                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId);
                }

                if (character.Level < Constants.MinimumRetiringLevel)
                {
                    throw new BadRequestException($"Level {Constants.MinimumRetiringLevel} is required to retire");
                }

                character.Experience = 0;
                character.Level = 1;
                character.ExperienceMultiplier += Constants.ExperienceMultiplierIncrease;
                character.HeadItemId = null;
                character.CapeItemId = null;
                character.BodyItemId = null;
                character.HandItemId = null;
                character.LegItemId = null;
                character.HorseHarnessItem = null;
                character.HorseItem = null;
                character.Weapon1Item = null;
                character.Weapon2Item = null;
                character.Weapon3Item = null;
                character.Weapon4Item = null;
                character.User!.LoomPoints += 1;

                await _db.SaveChangesAsync(cancellationToken);

                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}