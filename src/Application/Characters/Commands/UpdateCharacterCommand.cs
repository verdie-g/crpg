using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = default!;

        public class Validator : AbstractValidator<UpdateCharacterCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<UpdateCharacterCommand, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(UpdateCharacterCommand request, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId, request.UserId);
                }

                character.Name = request.Name;
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}