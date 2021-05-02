using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Strategus.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusBattleMercenariesQuery : IMediatorRequest<IList<StrategusBattleMercenaryViewModel>>
    {
        public int UserId { get; init; }
        public int BattleId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetStrategusBattleMercenariesQuery, IList<StrategusBattleMercenaryViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ICharacterClassModel _characterClassModel;

            public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassModel characterClassModel)
            {
                _db = db;
                _mapper = mapper;
                _characterClassModel = characterClassModel;
            }

            public async Task<Result<IList<StrategusBattleMercenaryViewModel>>> Handle(GetStrategusBattleMercenariesQuery req, CancellationToken cancellationToken)
            {
                var battle = await _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.Fighters)
                    .Include(b => b.Mercenaries).ThenInclude(m => m.Character!.User)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                // Mercenaries can only apply during the Hiring phase so return an error for preceding phases.
                if (battle.Phase == StrategusBattlePhase.Preparation)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                StrategusBattleFighter? fighter = battle.Fighters.FirstOrDefault(f => f.HeroId == req.UserId);
                // During the hiring phase, only the fighters can see the mercenaries.
                if (battle.Phase == StrategusBattlePhase.Hiring && fighter == null)
                {
                    return new(CommonErrors.HeroNotAFighter(req.UserId, req.BattleId));
                }

                // Return the mercenaries from the same side as the user during the hiring phase.
                var mercenaries = battle.Mercenaries
                    .Where(m => battle.Phase != StrategusBattlePhase.Hiring || m.Side == fighter!.Side)
                    .Select(m => new StrategusBattleMercenaryViewModel
                {
                    Id = m.Id,
                    User = _mapper.Map<UserPublicViewModel>(m.Character!.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = m.Character.Id,
                        Level = m.Character.Level,
                        Class = _characterClassModel.ResolveCharacterClass(m.Character.Statistics),
                    },
                    Side = m.Side,
                }).ToArray();

                return new(mercenaries);
            }
        }
    }
}
