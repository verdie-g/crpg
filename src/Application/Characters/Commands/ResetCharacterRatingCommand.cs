using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record ResetCharacterRatingCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<ResetCharacterRatingCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ResetCharacterRatingCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService,
            IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<CharacterViewModel>> Handle(ResetCharacterRatingCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            _characterService.ResetRating(character);

            _db.ActivityLogs.Add(_activityLogService.CreateCharacterRatingResetLog(character.UserId, character.Id));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' had his  character '{1} rating reset'", req.UserId, req.CharacterId);
            return new(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
