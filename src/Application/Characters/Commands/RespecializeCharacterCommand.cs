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
    public class RespecializeCharacterCommand : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<RespecializeCharacterCommand, CharacterViewModel>
        {
            private const float ExperiencePenalty = 0.5f;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterViewModel>> Handle(RespecializeCharacterCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                        c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
                if (character == null)
                {
                    return new Result<CharacterViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                character.Experience = (int)(character.Experience * ExperiencePenalty);
                character.Level = ExperienceTable.GetLevelForExperience(character.Experience);
                CharacterHelper.ResetCharacterStats(character, true);
                CharacterHelper.UnequipCharacterItems(character.Items);

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
            }
        }
    }
}
