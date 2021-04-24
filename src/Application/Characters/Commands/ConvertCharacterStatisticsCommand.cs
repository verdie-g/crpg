using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public record ConvertCharacterStatisticsCommand : IMediatorRequest<CharacterStatisticsViewModel>
    {
        public int CharacterId { get; init; }
        public int UserId { get; init; }
        public CharacterStatisticConversion Conversion { get; init; }

        public class Validator : AbstractValidator<ConvertCharacterStatisticsCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Conversion).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<ConvertCharacterStatisticsCommand, CharacterStatisticsViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterStatisticsViewModel>> Handle(ConvertCharacterStatisticsCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                    c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                if (req.Conversion == CharacterStatisticConversion.AttributesToSkills)
                {
                    if (character.Statistics.Attributes.Points < 1)
                    {
                        return new(CommonErrors.NotEnoughAttributePoints(1, character.Statistics.Attributes.Points));
                    }

                    character.Statistics.Attributes.Points -= 1;
                    character.Statistics.Skills.Points += 2;
                }
                else if (req.Conversion == CharacterStatisticConversion.SkillsToAttributes)
                {
                    if (character.Statistics.Skills.Points < 2)
                    {
                        return new(CommonErrors.NotEnoughSkillPoints(1, character.Statistics.Skills.Points));
                    }

                    character.Statistics.Skills.Points -= 2;
                    character.Statistics.Attributes.Points += 1;
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new(_mapper.Map<CharacterStatisticsViewModel>(character.Statistics));
            }
        }
    }
}
