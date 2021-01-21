using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class KickClanMemberCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfUserDoesntExist()
        {
            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = 1,
                ClanId = 2,
                KickedUserId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserIsNotInAClan()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = 2,
                KickedUserId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotInAClan, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserIsNotAClanMember()
        {
            var user = new User { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Users.Add(user);

            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotAClanMember, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfLeaderTriesToLeave()
        {
            var clan = new Clan();
            var user1 = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
            var user2 = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(user1, user2);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user1.Id,
                ClanId = clan.Id,
                KickedUserId = user1.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.ClanNeedLeader, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldDeleteClanIfLeaderIsTheLastMemberAndLeaving()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            Assert.IsEmpty(AssertDb.Clans);
            Assert.IsEmpty(AssertDb.ClanMembers);
        }

        [Test]
        public async Task ShouldLeaveClanIfUserKickedHimself()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            Assert.That(AssertDb.ClanMembers, Has.Exactly(0)
                .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
        }

        [Test]
        public async Task ShouldReturnErrorIfKickedUserDoesntExist()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfKickedUserIsNotInAClan()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan } };
            var kickedUser = new User();
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = kickedUser.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotInAClan, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfKickedUserIsNotAClanMember()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan } };
            var kickedUser = new User { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = kickedUser.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.UserNotAClanMember, result.Errors![0].Code);
        }

        [TestCase(ClanMemberRole.Member, ClanMemberRole.Admin)]
        [TestCase(ClanMemberRole.Admin, ClanMemberRole.Leader)]
        public async Task ShouldNotKickUserIfHisRoleIsHigher(ClanMemberRole userRole, ClanMemberRole kickedUserRole)
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            var kickedUser = new User { ClanMembership = new ClanMember { Clan = clan, Role = kickedUserRole } };
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                KickedUserId = kickedUser.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, result.Errors![0].Code);
        }

        [TestCase(ClanMemberRole.Admin, ClanMemberRole.Member)]
        [TestCase(ClanMemberRole.Leader, ClanMemberRole.Admin)]
        public async Task ShouldKickUser(ClanMemberRole userRole, ClanMemberRole kickedUserRole)
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            var kickedUser = new User { ClanMembership = new ClanMember { Clan = clan, Role = kickedUserRole } };
            ArrangeDb.Users.AddRange(user, kickedUser);
            await ArrangeDb.SaveChangesAsync();

            var result = await new KickClanMemberCommand.Handler(ActDb).Handle(new KickClanMemberCommand
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
