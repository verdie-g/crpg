using System.Collections.Generic;
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
    public class CreateCharacterCommand : IRequest<CharacterViewModel>
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        public class Validator : AbstractValidator<CreateCharacterCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<CreateCharacterCommand, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(CreateCharacterCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.UserId);
                }

                var newCharacter = new Character
                {
                    UserId = request.UserId,
                    Name = request.Name,
                    Experience = 0,
                    Level = 1,
                };

                user.Characters = new List<Character> { newCharacter };
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(newCharacter);
            }
        }
    }
}
