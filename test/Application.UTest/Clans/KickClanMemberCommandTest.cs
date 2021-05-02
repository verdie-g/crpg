using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class KickClanMemberCommandTest : TestBase
    {
        private static readonly IClanService ClanService = new ClanService();

        [Test]
        public async Task ShouldLeaveClanIfUserKickedHimself()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb, ClanService).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            Assert.That(AssertDb.ClanMembers, Has.Exactly(0)
                .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
        }

        [TestCase(ClanMemberRole.Member, ClanMemberRole.Officer)]
        [TestCase(ClanMemberRole.Officer, ClanMemberRole.Leader)]
        public async Task ShouldNotKickUserIfHisRoleIsHigher(ClanMemberRole userRole, ClanMemberRole kickedUserRole)
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            var kickedUser = new User { ClanMembership = new ClanMember { Clan = clan, Role = kickedUserRole } };
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb, ClanService).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = kickedUser.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, result.Errors![0].Code);
        }

        [TestCase(ClanMemberRole.Officer, ClanMemberRole.Member)]
        [TestCase(ClanMemberRole.Leader, ClanMemberRole.Officer)]
        public async Task ShouldKickUser(ClanMemberRole userRole, ClanMemberRole kickedUserRole)
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            var kickedUser = new User { ClanMembership = new ClanMember { Clan = clan, Role = kickedUserRole } };
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb, ClanService).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = kickedUser.Id,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            Assert.That(AssertDb.ClanMembers, Has.Exactly(1)
                .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
            Assert.That(AssertDb.ClanMembers, Has.Exactly(0)
                .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == kickedUser.Id));
        }
    }
}
