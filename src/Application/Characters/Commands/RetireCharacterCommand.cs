using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class RetireCharacterCommand : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<RetireCharacterCommand, CharacterViewModel>
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

            public async Task<Result<CharacterViewModel>> Handle(RetireCharacterCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
                if (character == null)
                {
                    return new Result<CharacterViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                if (character.Level < MinimumRetiringLevel)
                {
                    return new Result<CharacterViewModel>(CommonErrors.CharacterLevelRequirementNotMet(MinimumRetiringLevel, character.Level));
                }

                character.Generation += 1;
                character.Level = CharacterHelper.DefaultLevel;
                character.Experience = CharacterHelper.DefaultExperience;
                character.ExperienceMultiplier += ExperienceMultiplierIncrease;
                CharacterHelper.ResetCharacterStats(character);
                CharacterHelper.UnequipCharacterItems(character.Items);

                character.User!.HeirloomPoints += 1;

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
            }
        }
    }
}
