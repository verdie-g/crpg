using System.Linq;
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
    public class InviteClanMemberCommandTest : TestBase
    {
        private static readonly IClanService ClanService = new ClanService();

        [Test]
        public async Task ShouldReturnErrorIfUserNotFound()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = 1,
                ClanId = clan.Id,
                InviteeUserId = 1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task IfRequestShouldCreateOneIfNotAlreadyExists()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                InviteeUserId = user.Id,
            }, CancellationToken.None);

            var invitation = res.Data!;
            Assert.IsNull(res.Errors);
            Assert.NotZero(invitation.Id);
            Assert.AreEqual(clan.Id, invitation.ClanId);
            Assert.AreEqual(user.Id, invitation.InviteeUser.Id);
            Assert.AreEqual(user.Id, invitation.InviterUser.Id);
            Assert.AreEqual(ClanInvitationType.Request, invitation.Type);
            Assert.AreEqual(ClanInvitationStatus.Pending, invitation.Status);
        }

        [Test]
        public async Task IfRequestShouldReturnExistingPendingRequest()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var user = new User();
            ArrangeDb.Users.Add(user);
            var invitation = new ClanInvitation
            {
                Clan = clan,
                InviteeUser = user,
                InviterUser = user,
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            };
            ArrangeDb.ClanInvitations.Add(invitation);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                InviteeUserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(invitation.Id, res.Data!.Id);
        }

        [Test]
        public async Task IfOfferButInviterNotInAClanShouldReturnError()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitee = new User();
            var inviter = new User();
            ArrangeDb.Users.AddRange(invitee, inviter);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = inviter.Id,
                ClanId = clan.Id,
                InviteeUserId = invitee.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotInAClan, res.Errors![0].Code);
        }

        [Test]
        public async Task IfOfferButInviterNotInTheClanShouldReturnError()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitee = new User();
            var inviter = new User { ClanMembership = new ClanMember { Clan = new Clan(), Role = ClanMemberRole.Admin } };
            ArrangeDb.Users.AddRange(invitee, inviter);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = inviter.Id,
                ClanId = clan.Id,
                InviteeUserId = invitee.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotAClanMember, res.Errors![0].Code);
        }

        [Test]
        public async Task IfOfferButInviterNotAdminOrLeaderShouldReturnError()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitee = new User();
            var inviter = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(invitee, inviter);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = inviter.Id,
                ClanId = clan.Id,
                InviteeUserId = invitee.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
        }

        [Test]
        public async Task IfOfferShouldReturnExistingPendingOffer()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitee = new User();
            var inviter = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Admin } };
            ArrangeDb.Users.AddRange(invitee, inviter);
            var offer = new ClanInvitation
            {
                Clan = clan,
                InviteeUser = invitee,
                InviterUser = inviter,
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Pending,
            };
            ArrangeDb.ClanInvitations.Add(offer);
            await ArrangeDb.SaveChangesAsync();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = inviter.Id,
                ClanId = clan.Id,
                InviteeUserId = invitee.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(offer.Id, res.Data!.Id);
        }

        [Test]
        public async Task IfOfferShouldCreateOneIfNotAlreadyExists()
        {
            var clan = new Clan();
            ArrangeDb.Clans.Add(clan);
            var invitee = new User();
            var inviter = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Admin } };
            ArrangeDb.Users.AddRange(invitee, inviter);
            await ArrangeDb.SaveChangesAsync();

            var a = ActDb.Users.ToArray();

            var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
            {
                UserId = inviter.Id,
                ClanId = clan.Id,
                InviteeUserId = invitee.Id,
            }, CancellationToken.None);

            var invitation = res.Data!;
            Assert.IsNull(res.Errors);
            Assert.NotZero(invitation.Id);
            Assert.AreEqual(clan.Id, invitation.ClanId);
            Assert.AreEqual(invitee.Id, invitation.InviteeUser.Id);
            Assert.AreEqual(inviter.Id, invitation.InviterUser.Id);
            Assert.AreEqual(ClanInvitationType.Offer, invitation.Type);
            Assert.AreEqual(ClanInvitationStatus.Pending, invitation.Status);
        }
    }
}
