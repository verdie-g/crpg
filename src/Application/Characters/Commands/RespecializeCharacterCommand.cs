using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class RespecializeCharacterCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<RespecializeCharacterCommand, CharacterViewModel>
        {
            private const float ExperiencePenalty = 0.5f;

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(RespecializeCharacterCommand request, CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                        c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);
                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId);
                }

                character.Experience = (int)(character.Experience * ExperiencePenalty);
                character.Level = ExperienceTable.GetLevelForExperience(character.Experience);
                CharacterHelper.ResetCharacterStats(character, true);
                CharacterHelper.UnequipCharacterItems(character.Items);

                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}