using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record SetCharacterForTournamentCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<SetCharacterForTournamentCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<SetCharacterForTournamentCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IExperienceTable _experienceTable;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService,
            IExperienceTable experienceTable, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _experienceTable = experienceTable;
            _constants = constants;
        }

        public async Task<Result<CharacterViewModel>> Handle(SetCharacterForTournamentCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
            if (character == null)
            {
                return new Result<CharacterViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (character.Generation != 0)
            {
                return new Result<CharacterViewModel>(CommonErrors.CharacterGenerationRequirement(req.CharacterId, req.UserId, 0));
            }

            character.ForTournament = true;
            character.Level = _constants.TournamentLevel;
            character.Experience = _experienceTable.GetExperienceForLevel(character.Level);
            _characterService.ResetCharacterCharacteristics(character, respecialization: true);

            if (character.User!.ActiveCharacterId == character.Id)
            {
                character.User!.ActiveCharacterId = null;
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' set character '{1}' for tournaments", req.UserId, req.CharacterId);
            return new(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
