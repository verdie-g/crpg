using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class RespecializeCharacterCommand : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<RespecializeCharacterCommand, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly CharacterService _characterService;
            private readonly ExperienceTable _experienceTable;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, IMapper mapper, CharacterService characterService,
                ExperienceTable experienceTable, Constants constants)
            {
                _db = db;
                _mapper = mapper;
                _characterService = characterService;
                _experienceTable = experienceTable;
                _constants = constants;
            }

            public async Task<Result<CharacterViewModel>> Handle(RespecializeCharacterCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.EquippedItems)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
                if (character == null)
                {
                    return new Result<CharacterViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                character.Experience = (int)MathHelper.ApplyPolynomialFunction(character.Experience, _constants.RespecializeExperiencePenaltyCoefs);
                character.Level = _experienceTable.GetLevelForExperience(character.Experience);
                character.EquippedItems.Clear(); // Unequip all items.
                _characterService.ResetCharacterStats(character, true);

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
            }
        }
    }
}
