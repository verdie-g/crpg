﻿using AutoMapper;
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

public record RespecializeCharacterCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<RespecializeCharacterCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespecializeCharacterCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IExperienceTable _experienceTable;
        private readonly IActivityLogService _activityLogService;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService,
            IExperienceTable experienceTable, IActivityLogService activityLogService, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _experienceTable = experienceTable;
            _activityLogService = activityLogService;
            _constants = constants;
        }

        public async Task<Result<CharacterViewModel>> Handle(RespecializeCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (!character.ForTournament)
            {
                character.Experience = (int)MathHelper.ApplyPolynomialFunction(character.Experience,
                    _constants.RespecializeExperiencePenaltyCoefs);
                user.Gold = (int)(user.Gold - (character.Experience / 4420824f) * 5000);
                character.Level = _experienceTable.GetLevelForExperience(character.Experience);
            }

            _characterService.ResetCharacterCharacteristics(character, true);

            _db.ActivityLogs.Add(_activityLogService.CreateCharacterRespecializedLog(character.UserId, character.Id));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' respecialized character '{1}'", req.UserId, req.CharacterId);
            return new(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
