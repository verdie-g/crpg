using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Common.Services
{
    internal interface IClanService
    {
        Task<Result<User>> GetClanMember(ICrpgDbContext db, int userId, int clanId, CancellationToken cancellationToken);
        Error? CheckClanMembership(User user, int clanId);
        Task<Result> LeaveClan(ICrpgDbContext db, ClanMember member, CancellationToken cancellationToken);
    }

    internal class ClanService : IClanService
    {
        public async Task<Result<User>> GetClanMember(ICrpgDbContext db, int userId, int clanId, CancellationToken cancellationToken)
        {
            var user = await db.Users
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                return new Result<User>(CommonErrors.UserNotFound(userId));
            }

            var error = CheckClanMembership(user, clanId);
            return error != null ? new Result<User>(error) : new Result<User>(user);
        }

        public Error? CheckClanMembership(User user, int clanId)
        {
            if (user.ClanMembership == null)
            {
                return CommonErrors.UserNotInAClan(user.Id);
            }

            if (user.ClanMembership.ClanId != clanId)
            {
                return CommonErrors.UserNotAClanMember(user.Id, clanId);
            }

            return null;
        }

        public async Task<Result> LeaveClan(ICrpgDbContext db, ClanMember member, CancellationToken cancellationToken)
        {
            // If user is leader and wants to leave, he needs to be the last member or have designated a new leader first.
            if (member.Role == ClanMemberRole.Leader)
            {
                await db.Entry(member)
                    .Reference(m => m.Clan!)
                    .Query()
                    .Include(c => c.Members)
                    .LoadAsync(cancellationToken);

                if (member.Clan!.Members.Count > 1)
                {
                    return new Result(CommonErrors.ClanNeedLeader(member.ClanId));
                }

                db.Clans.Remove(member.Clan);
            }

            db.ClanMembers.Remove(member);
            return Result.NoErrors;
        }
    }
}
