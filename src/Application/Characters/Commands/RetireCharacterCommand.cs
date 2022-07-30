using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record RetireCharacterCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<RetireCharacterCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RetireCharacterCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _constants = constants;
        }

        public async Task<Result<CharacterViewModel>> Handle(RetireCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .Include(c => c.EquippedItems)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (character.Level < _constants.MinimumRetirementLevel)
            {
                return new(CommonErrors.CharacterLevelRequirementNotMet(_constants.MinimumRetirementLevel, character.Level));
            }

            character.Generation += 1;
            character.Level = _constants.MinimumLevel;
            character.Experience = 0;
            character.ExperienceMultiplier = MathHelper.ApplyPolynomialFunction(character.Generation, _constants.ExperienceMultiplierForGenerationCoefs);
            character.EquippedItems.Clear();
            _characterService.ResetCharacterCharacteristics(character);

            character.User!.HeirloomPoints += 1;

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' retired character '{1}'", req.UserId, req.CharacterId);
            return new(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
