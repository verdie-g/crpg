using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters.Commands
{
    public class CreateCharacterCommand : IRequest<CharacterModelView>
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        public class Validator : AbstractValidator<CreateCharacterCommand>
        {
            public Validator()
            {
                RuleFor(e => e.Name).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<CreateCharacterCommand, CharacterModelView>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterModelView> Handle(CreateCharacterCommand request,
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

                user.Characters = new List<Character> {newCharacter};
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterModelView>(newCharacter);
            }
        }
    }
}
