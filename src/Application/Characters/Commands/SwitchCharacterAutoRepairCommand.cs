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
    public class SwitchCharacterAutoRepairCommand : IMediatorRequest
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public bool AutoRepair { get; set; }

        public class Handler : IMediatorRequestHandler<SwitchCharacterAutoRepairCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result> Handle(SwitchCharacterAutoRepairCommand req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                    c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    return new Result(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                character.AutoRepair = req.AutoRepair;

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterStatisticsViewModel>(_mapper.Map<CharacterStatisticsViewModel>(character.Statistics));
            }
        }
    }
}
