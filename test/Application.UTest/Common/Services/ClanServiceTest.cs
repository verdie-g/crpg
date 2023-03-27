using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ClanServiceTest : TestBase
{
    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserNotFound()
    {
        ClanService clanService = new();
        var res = await clanService.GetClanMember(ActDb, 1, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserIsNotInAClan()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var res = await clanService.GetClanMember(ActDb, user.Id, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotInAClan));
    }

    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserIsNotAClanMember()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var res = await clanService.GetClanMember(ActDb, user.Id, 3, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotAClanMember));
    }

    [Test]
    public async Task GetClanMemberShouldNotReturnErrorIfUserIsAClanMember()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var res = await clanService.GetClanMember(ActDb, user.Id, user.ClanMembership.ClanId, CancellationToken.None);
        Assert.That(res.Errors, Is.Null);
    }

    [Test]
    public async Task JoinClanShouldDeleteInvitationRequestsAndDeclineInvitationOffers()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation[] invitations =
        {
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Pending,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Declined,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = user,
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Accepted,
            },
        };
        ArrangeDb.ClanInvitations.AddRange(invitations);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var u = await ActDb.Users.FirstAsync(u => u.Id == user.Id);
        var res = await clanService.JoinClan(ActDb, u, clan.Id, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
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
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new()
        {
            Members =
            {
                new ClanMember { User = user, Role = ClanMemberRole.Leader },
                new ClanMember { User = new User(), Role = ClanMemberRole.Member },
            },
        };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNeedLeader));
    }

    [Test]
    public async Task LeaveClanShouldLeaveClanIfMemberLeaderButLastMember()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new() { Members = { new ClanMember { User = user, Role = ClanMemberRole.Leader } } };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
        Assert.That(AssertDb.Clans, Has.Exactly(0).Matches<Clan>(c => c.Id == clan.Id));
        Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
    }

    [Test]
    public async Task LeaveClanShouldWork()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new() { Members = { new ClanMember { User = user, Role = ClanMemberRole.Member } } };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new();
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
        Assert.That(AssertDb.Clans, Has.Exactly(1).Matches<Clan>(c => c.Id == clan.Id));
        Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
    }
}
