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

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterCommand : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = default!;
        public string BodyProperties { get; set; } = string.Empty;
        public CharacterGender Gender { get; set; }

        public class Validator : AbstractValidator<UpdateCharacterCommand>
        {
            private const int MinimumCharacterNameLength = 2;
            private const int MaximumCharacterNameLength = 32;
            private const int BodyPropertiesLength = 128;

            public Validator()
            {
                RuleFor(c => c.Name)
                    .MinimumLength(MinimumCharacterNameLength)
                    .MaximumLength(MaximumCharacterNameLength);

                RuleFor(c => c.BodyProperties).Length(BodyPropertiesLength);

                RuleFor(c => c.Gender).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateCharacterCommand, CharacterViewModel>
        {
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
                character.BodyProperties = req.BodyProperties;
                character.Gender = req.Gender;
                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterViewModel>(_mapper.Map<CharacterViewModel>(character));
            }
        }
    }
}
