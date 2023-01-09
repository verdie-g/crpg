using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands;

public record GetGameUserTournamentCommand : IMediatorRequest<GameUserViewModel>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = default!;

    internal class Handler : IMediatorRequestHandler<GetGameUserTournamentCommand, GameUserViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<GameUserViewModel>> Handle(GetGameUserTournamentCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Characters.Where(c => c.ForTournament).Take(1))
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                    cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.Platform, req.PlatformUserId));
            }

            if (user.Characters.Count == 0)
            {
                return new(CommonErrors.CharacterForTournamentNotFound());
            }

            // Load items in separate query to avoid cartesian explosion if character has many items equipped.
            await _db.Entry(user.Characters[0])
                .Collection(c => c.EquippedItems)
                .Query()
                .Include(ei => ei.UserItem)
                .LoadAsync(cancellationToken);

            var gameUser = _mapper.Map<GameUserViewModel>(user);
            return new(gameUser);
        }
    }
}
