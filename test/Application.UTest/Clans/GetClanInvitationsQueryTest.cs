using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class GetClanInvitationsQueryTest : TestBase
    {
        private static readonly IClanService ClanService = new ClanService();

        [Test]
        public async Task ShouldReturnErrorIfUserNotFound()
        {
            var res = await new GetClanInvitationsQuery.Handler(ActDb, Mapper, ClanService).Handle(new GetClanInvitationsQuery
            {
                UserId = 1,
                ClanId = 2,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserNotAdmin()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan
            {
                Members = { new ClanMember { User = user, Role = ClanMemberRole.Member } },
            };
            ArrangeDb.Clans.Add(clan);
            await ArrangeDb.SaveChangesAsync();

            var res = await new GetClanInvitationsQuery.Handler(ActDb, Mapper, ClanService).Handle(new GetClanInvitationsQuery
            {
                UserId = user.Id,
                ClanId = clan.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnAllClanInvitationsIfTypesAndStatusesEmpty()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan
            {
                Members = { new ClanMember { User = user, Role = ClanMemberRole.Admin } },
            };
            ArrangeDb.Clans.Add(clan);
            var invitations = new[]
            {
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Accepted,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Declined,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = new Clan(),
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                },
            };
            ArrangeDb.ClanInvitations.AddRange(invitations);
            await ArrangeDb.SaveChangesAsync();

            var res = await new GetClanInvitationsQuery.Handler(ActDb, Mapper, ClanService).Handle(new GetClanInvitationsQuery
            {
                UserId = user.Id,
                ClanId = clan.Id,
                Types = Array.Empty<ClanInvitationType>(),
                Statuses = Array.Empty<ClanInvitationStatus>(),
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(4, res.Data!.Count);
        }

        [Test]
        public async Task ShouldReturnSpecifiedTypeAndStatusOnly()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var clan = new Clan
            {
                Members = { new ClanMember { User = user, Role = ClanMemberRole.Admin } },
            };
            ArrangeDb.Clans.Add(clan);
            var invitations = new[]
            {
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Accepted,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Declined,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = new Clan(),
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Accepted,
                },
                new ClanInvitation
                {
                    Clan = clan,
                    Invitee = new User(),
                    Inviter = new User(),
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                },
            };
            ArrangeDb.ClanInvitations.AddRange(invitations);
            await ArrangeDb.SaveChangesAsync();

            var res = await new GetClanInvitationsQuery.Handler(ActDb, Mapper, ClanService).Handle(new GetClanInvitationsQuery
            {
                UserId = user.Id,
                ClanId = clan.Id,
                Types = new[] { ClanInvitationType.Offer },
                Statuses = new[] { ClanInvitationStatus.Pending },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(1, res.Data!.Count);
            Assert.AreEqual(ClanInvitationStatus.Pending, res.Data![0].Status);
        }
    }
}
