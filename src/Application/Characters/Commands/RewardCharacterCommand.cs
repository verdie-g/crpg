using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record RewardCharacterCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }
    public int Experience { get; init; }
    public bool AutoRetire { get; init; }

    public class Validator : AbstractValidator<RewardCharacterCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Experience).GreaterThan(0);
        }
    }

    internal class Handler : IMediatorRequestHandler<RewardCharacterCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RewardCharacterCommand>();

        private readonly ICharacterService _characterService;
        private readonly IExperienceTable _experienceTable;
        private readonly IActivityLogService _activityLogService;
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly Constants _constants;

        public Handler(
            ICharacterService characterService,
            IExperienceTable experienceTable,
            IActivityLogService activityLogService,
            ICrpgDbContext db,
            IMapper mapper,
            Constants constants)
        {
            _characterService = characterService;
            _experienceTable = experienceTable;
            _activityLogService = activityLogService;
            _db = db;
            _mapper = mapper;
            _constants = constants;
        }

        public async Task<Result<CharacterViewModel>> Handle(RewardCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (req.AutoRetire)
            {
                int totalExperienceForRetirementLevel = _experienceTable.GetExperienceForLevel(_constants.MinimumRetirementLevel);
                int remainingExperienceToGive = req.Experience;

                while (remainingExperienceToGive > 0)
                {
                    int experienceNeededToRetirementLevel = Math.Max(totalExperienceForRetirementLevel - character.Experience, 0);
                    (int experienceToGive, bool retirementLevelReached) = remainingExperienceToGive >= experienceNeededToRetirementLevel
                        ? (experienceNeededToRetirementLevel, true)
                        : (remainingExperienceToGive, false);
                    _characterService.GiveExperience(character, experienceToGive, useExperienceMultiplier: false);
                    if (retirementLevelReached)
                    {
                        _characterService.Retire(character);
                    }

                    remainingExperienceToGive -= experienceToGive;
                }
            }
            else
            {
                _characterService.GiveExperience(character, req.Experience, useExperienceMultiplier: false);
            }

            _db.ActivityLogs.Add(_activityLogService.CreateCharacterRewardedLog(req.UserId, req.CharacterId, req.Experience));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Character '{0}' rewarded", req.CharacterId);

            return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
