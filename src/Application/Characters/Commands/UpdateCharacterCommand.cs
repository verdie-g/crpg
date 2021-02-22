using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterCommand : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = default!;

        public class Validator : AbstractValidator<UpdateCharacterCommand>
        {
            private const int MinimumCharacterNameLength = 2;
            private const int MaximumCharacterNameLength = 32;

            public Validator()
            {
                RuleFor(c => c.Name)
                    .MinimumLength(MinimumCharacterNameLength)
                    .MaximumLength(MaximumCharacterNameLength);
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateCharacterCommand, CharacterViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateCharacterCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterViewModel>> Handle(UpdateCharacterCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

                if (character == null)
                {
                    return new Result<CharacterViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                character.Name = req.Name;
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' updated character '{1}'", req.UserId, req.CharacterId);
                return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
            }
        }
    }
}
