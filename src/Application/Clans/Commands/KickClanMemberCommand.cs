using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands
{
    public class KickClanMemberCommand : IMediatorRequest
    {
        public int UserId { get; set; }
        public int ClanId { get; set; }
        public int KickedUserId { get; set; }

        internal class Handler : IMediatorRequestHandler<KickClanMemberCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<KickClanMemberCommand>();

            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(KickClanMemberCommand req, CancellationToken cancellationToken)
            {
                var userRes = await GetClanMember(req.UserId, req.ClanId, cancellationToken);
                if (userRes.Errors != null)
                {
                    return new Result(userRes.Errors);
                }

                User user = userRes.Data!;
                if (req.UserId == req.KickedUserId) // User is leaving the clan
                {
                    // If user is leader and wants to leave, he needs to be the last member or have designated a new leader first.
                    if (user.ClanMembership!.Role == ClanMemberRole.Leader)
                    {
                        Clan clan = _db.Clans
                            .Include(c => c.Members)
                            .First(c => c.Id == req.ClanId);

                        if (clan.Members.Count > 1)
                        {
                            return new Result(CommonErrors.ClanNeedLeader(req.ClanId));
                        }

                        _db.Clans.Remove(clan);
                    }

                    _db.ClanMembers.Remove(user.ClanMembership);
                    await _db.SaveChangesAsync(cancellationToken);
                    Logger.LogInformation("User '{0}' left clan '{1}'", req.UserId, req.ClanId);
                    return new Result();
                }

                var kickedUserRes = await GetClanMember(req.KickedUserId, req.ClanId, cancellationToken);
                if (kickedUserRes.Errors != null)
                {
                    return new Result(kickedUserRes.Errors);
                }

                User kickedUser = kickedUserRes.Data!;
                if (user.ClanMembership!.Role < kickedUser.ClanMembership!.Role)
                {
                    return new Result(CommonErrors.ClanMemberRoleNotMet(req.UserId, kickedUser.ClanMembership.Role + 1,
                        user.ClanMembership.Role));
                }

                _db.ClanMembers.Remove(kickedUser.ClanMembership);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' kicked user '{1}' out of clan '{2}'", req.UserId,
                    req.KickedUserId, req.ClanId);
                return new Result();
            }

            private async Task<Result<User>> GetClanMember(int userId, int clanId, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (user == null)
                {
                    return new Result<User>(CommonErrors.UserNotFound(userId));
                }

                if (user.ClanMembership == null)
                {
                    return new Result<User>(CommonErrors.UserNotInAClan(userId));
                }

                if (user.ClanMembership.ClanId != clanId)
                {
                    return new Result<User>(CommonErrors.UserNotAClanMember(userId, clanId));
                }

                return new Result<User>(user);
            }
        }
    }
}
