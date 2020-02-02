using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Common.Validators;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters.Commands
{
    public class CreateCharacterCommand : IRequest<CharacterModelView>
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        public class Validator : AbstractValidator<CreateCharacterCommand>
        {
            public Validator(ITrpgDbContext db)
            {
                Include(new KeyValidator<CreateCharacterCommand, User, int>(db.Users, c => c.UserId));
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

            public async Task<CharacterModelView> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
            {
                var character = _db.Characters.Add(new Character
                {
                    UserId = request.UserId,
                    Name = request.Name,
                    Experience = 0,
                    Level = 1,
                });
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterModelView>(character.Entity);
            }
        }
    }
}
