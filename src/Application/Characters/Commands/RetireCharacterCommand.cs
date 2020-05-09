using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
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
            private const int MinimumRetiringLevel = 31;
            private const float ExperienceMultiplierIncrease = 0.05f;

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

                if (character.Level < MinimumRetiringLevel)
                {
                    throw new BadRequestException($"Level {MinimumRetiringLevel} is required to retire");
                }

                character.Experience = 0;
                character.Level = 1;
                character.ExperienceMultiplier += ExperienceMultiplierIncrease;
                UnequipItems(character.Items);
                character.User!.LoomPoints += 1;

                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }

            private void UnequipItems(CharacterItems items)
            {
                items.HeadItemId = null;
                items.CapeItemId = null;
                items.BodyItemId = null;
                items.HandItemId = null;
                items.LegItemId = null;
                items.HorseHarnessItem = null;
                items.HorseItem = null;
                items.Weapon1Item = null;
                items.Weapon2Item = null;
                items.Weapon3Item = null;
                items.Weapon4Item = null;
            }
        }
    }
}