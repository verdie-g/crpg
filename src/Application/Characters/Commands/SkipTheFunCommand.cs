using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Characters.Commands
{
    public class SkipTheFunCommand : IMediatorRequest
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<SkipTheFunCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly ICharacterService _characterService;
            private readonly IExperienceTable _experienceTable;
            private readonly ILogger<SkipTheFunCommand> _logger;

            public Handler(ICrpgDbContext db, ICharacterService characterService,
                IExperienceTable experienceTable, ILogger<SkipTheFunCommand> logger)
            {
                _db = db;
                _characterService = characterService;
                _experienceTable = experienceTable;
                _logger = logger;
            }

            public async Task<Result> Handle(SkipTheFunCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                    c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    return new Result(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                // TODO: allow skip the fun on already skipped the fun character?

                character.SkippedTheFun = true;
                character.Level = 30;
                character.Experience = _experienceTable.GetExperienceForLevel(character.Level);
                _characterService.ResetCharacterStats(character, respecialization: true);

                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User '{0}' skipped then fun character '{1}'", req.UserId, req.CharacterId);
                return new Result();
            }
        }
    }
}
