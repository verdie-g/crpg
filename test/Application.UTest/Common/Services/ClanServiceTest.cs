using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class ClanServiceTest : TestBase
    {
        [Test]
        public async Task GetClanMemberShouldReturnErrorIfUserNotFound()
        {
            var clanService = new ClanService();
            var res = await clanService.GetClanMember(ActDb, 1, 2, CancellationToken.None);
            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task GetClanMemberShouldReturnErrorIfUserIsNotInAClan()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var res = await clanService.GetClanMember(ActDb, user.Id, 2, CancellationToken.None);
            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotInAClan, res.Errors![0].Code);
        }

        [Test]
        public async Task GetClanMemberShouldReturnErrorIfUserIsNotAClanMember()
        {
            var user = new User { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var res = await clanService.GetClanMember(ActDb, user.Id, 3, CancellationToken.None);
            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotAClanMember, res.Errors![0].Code);
        }

        [Test]
        public async Task GetClanMemberShouldNotReturnErrorIfUserIsAClanMember()
        {
            var user = new User { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var res = await clanService.GetClanMember(ActDb, user.Id, user.ClanMembership.ClanId, CancellationToken.None);
            Assert.IsNull(res.Errors);
        }

        [Test]
        public async Task JoinClanShouldDeleteInvitationRequestsAndDeclineInvitationOffers()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitations = new[]
            {
                new ClanInvitation
                {
                    Clan = new Clan(),
                    InviteeUser = user,
                    InviterUser = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = new Clan(),
                    InviteeUser = user,
                    InviterUser = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Declined,
                },
                new ClanInvitation
                {
                    Clan = new Clan(),
                    InviteeUser = user,
                    InviterUser = user,
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = new Clan(),
                    InviteeUser = user,
                    InviterUser = new User(),
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Accepted,
                },
            };
            ArrangeDb.ClanInvitations.AddRange(invitations);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var u = await ActDb.Users.FirstAsync(u => u.Id == user.Id);
            var res = await clanService.JoinClan(ActDb, u, clan.Id, CancellationToken.None);
            await ActDb.SaveChangesAsync();

            Assert.IsNull(res.Errors);
            Assert.That(AssertDb.ClanInvitations, Has.Exactly(2)
                .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Offer && ci.Status == ClanInvitationStatus.Declined));
            Assert.That(AssertDb.ClanInvitations, Has.Exactly(0)
                .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Request && ci.Status == ClanInvitationStatus.Pending));
            Assert.That(AssertDb.ClanInvitations, Has.Exactly(1)
                .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Request && ci.Status == ClanInvitationStatus.Accepted));
        }

        [Test]
        public async Task LeaveClanShouldReturnErrorIfMemberLeaderAndNotLastMember()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan
            {
                Members =
                {
                    new ClanMember { User = user, Role = ClanMemberRole.Leader },
                    new ClanMember { User = new User(), Role = ClanMemberRole.Member },
                },
            };
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
            var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
            await ActDb.SaveChangesAsync();

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanNeedLeader, res.Errors![0].Code);
        }

        [Test]
        public async Task LeaveClanShouldLeaveClanIfMemberLeaderButLastMember()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan { Members = { new ClanMember { User = user, Role = ClanMemberRole.Leader } } };
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
            var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
            await ActDb.SaveChangesAsync();

            Assert.IsNull(res.Errors);
            Assert.That(AssertDb.Clans, Has.Exactly(0).Matches<Clan>(c => c.Id == clan.Id));
            Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
        }

        [Test]
        public async Task LeaveClanShouldWork()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan { Members = { new ClanMember { User = user, Role = ClanMemberRole.Member } } };
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var clanService = new ClanService();
            var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
            var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
            await ActDb.SaveChangesAsync();

            Assert.IsNull(res.Errors);
            Assert.That(AssertDb.Clans, Has.Exactly(1).Matches<Clan>(c => c.Id == clan.Id));
            Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
        }
    }
}
