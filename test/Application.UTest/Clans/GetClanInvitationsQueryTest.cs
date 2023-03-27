using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfUserNotOfficer()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new()
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
    }

    [Test]
    public async Task ShouldReturnAllClanInvitationsIfTypesAndStatusesEmpty()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new()
        {
            Members = { new ClanMember { User = user, Role = ClanMemberRole.Officer } },
        };
        ArrangeDb.Clans.Add(clan);
        ClanInvitation[] invitations =
        {
            new()
            {
                Clan = clan,
                Invitee = new User(),
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Accepted,
            },
            new()
            {
                Clan = clan,
                Invitee = new User(),
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Declined,
            },
            new()
            {
                Clan = clan,
                Invitee = new User(),
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Pending,
            },
            new()
            {
                Clan = clan,
                Invitee = new User(),
                Inviter = new User(),
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            },
            new()
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldReturnSpecifiedTypeAndStatusOnly()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new()
        {
            Members = { new ClanMember { User = user, Role = ClanMemberRole.Officer } },
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Count, Is.EqualTo(1));
        Assert.That(res.Data![0].Status, Is.EqualTo(ClanInvitationStatus.Pending));
    }
}
