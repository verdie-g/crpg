using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class ConvertCharacterStatisticsCommand : IRequest<CharacterStatisticsViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public CharacterStatisticConversion Conversion { get; set; }

        public class Validator : AbstractValidator<ConvertCharacterStatisticsCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Conversion).IsInEnum();
            }
        }

        public class Handler : IRequestHandler<ConvertCharacterStatisticsCommand, CharacterStatisticsViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterStatisticsViewModel> Handle(ConvertCharacterStatisticsCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                    c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), req.CharacterId);
                }

                if (req.Conversion == CharacterStatisticConversion.AttributesToSkills)
                {
                    if (character.Statistics.Attributes.Points < 1)
                    {
                        throw new BadRequestException("Not enough attribute points");
                    }

                    character.Statistics.Attributes.Points -= 1;
                    character.Statistics.Skills.Points += 2;
                }
                else if (req.Conversion == CharacterStatisticConversion.SkillsToAttributes)
                {
                    if (character.Statistics.Skills.Points < 2)
                    {
                        throw new BadRequestException("Not enough skill points");
                    }

                    character.Statistics.Skills.Points -= 2;
                    character.Statistics.Attributes.Points += 1;
                }

                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterStatisticsViewModel>(character.Statistics);
            }
        }
    }
}