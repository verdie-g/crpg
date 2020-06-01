using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class RetireCharacterCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<RetireCharacterCommand, CharacterViewModel>
        {
            private const int MinimumRetiringLevel = 31;
            private const float ExperienceMultiplierIncrease = 0.03f;

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
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);
                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId);
                }

                if (character.Level < MinimumRetiringLevel)
                {
                    throw new BadRequestException($"Level {MinimumRetiringLevel} is required to retire");
                }

                character.Level = CharacterHelper.DefaultLevel;
                character.Experience = CharacterHelper.DefaultExperience;
                character.ExperienceMultiplier += ExperienceMultiplierIncrease;
                CharacterHelper.ResetCharacterStats(character);
                CharacterHelper.UnequipCharacterItems(character.Items);

                character.User!.HeirloomPoints += 1;

                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}