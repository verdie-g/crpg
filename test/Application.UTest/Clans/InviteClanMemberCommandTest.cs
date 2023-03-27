using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class InviteClanMemberCommandTest : TestBase
{
    private static readonly IClanService ClanService = new ClanService();

    [Test]
    public async Task ShouldReturnErrorIfUserNotFound()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = 1,
            ClanId = clan.Id,
            InviteeId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task IfRequestShouldReturnErrorIfUserAlreadyInTheClan()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            InviteeId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserAlreadyInTheClan));
    }

    [Test]
    public async Task IfRequestShouldCreateOneIfNotAlreadyExists()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            InviteeId = user.Id,
        }, CancellationToken.None);

        var invitation = res.Data!;
        Assert.That(res.Errors, Is.Null);
        Assert.That(invitation.Id, Is.Not.Zero);
        Assert.That(invitation.Invitee.Id, Is.EqualTo(user.Id));
        Assert.That(invitation.Inviter.Id, Is.EqualTo(user.Id));
        Assert.That(invitation.Type, Is.EqualTo(ClanInvitationType.Request));
        Assert.That(invitation.Status, Is.EqualTo(ClanInvitationStatus.Pending));
    }

    [Test]
    public async Task IfRequestShouldReturnExistingPendingRequest()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user = new();
        ArrangeDb.Users.Add(user);
        ClanInvitation invitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(invitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            InviteeId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(invitation.Id));
    }

    [Test]
    public async Task IfOfferButInviteeAlreadyInTheClanShouldReturnError()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        User inviter = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Officer } };
        ArrangeDb.Users.AddRange(invitee, inviter);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserAlreadyInTheClan));
    }

    [Test]
    public async Task IfOfferButInviterNotInAClanShouldReturnError()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new();
        User inviter = new();
        ArrangeDb.Users.AddRange(invitee, inviter);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotInAClan));
    }

    [Test]
    public async Task IfOfferButInviterNotInTheClanShouldReturnError()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new();
        User inviter = new() { ClanMembership = new ClanMember { Clan = new Clan(), Role = ClanMemberRole.Officer } };
        ArrangeDb.Users.AddRange(invitee, inviter);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotAClanMember));
    }

    [Test]
    public async Task IfOfferButInviterNotOfficerOrLeaderShouldReturnError()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new();
        User inviter = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        ArrangeDb.Users.AddRange(invitee, inviter);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
    }

    [Test]
    public async Task IfOfferShouldReturnExistingPendingOffer()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new();
        User inviter = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Officer } };
        ArrangeDb.Users.AddRange(invitee, inviter);
        ClanInvitation offer = new()
        {
            Clan = clan,
            Invitee = invitee,
            Inviter = inviter,
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(offer.Id));
    }

    [Test]
    public async Task IfOfferShouldCreateOneIfNotAlreadyExists()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User invitee = new();
        User inviter = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Officer } };
        ArrangeDb.Users.AddRange(invitee, inviter);
        await ArrangeDb.SaveChangesAsync();

        var res = await new InviteClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new InviteClanMemberCommand
        {
            UserId = inviter.Id,
            ClanId = clan.Id,
            InviteeId = invitee.Id,
        }, CancellationToken.None);

        var invitation = res.Data!;
        Assert.That(res.Errors, Is.Null);
        Assert.That(invitation.Id, Is.Not.Zero);
        Assert.That(invitation.Invitee.Id, Is.EqualTo(invitee.Id));
        Assert.That(invitation.Inviter.Id, Is.EqualTo(inviter.Id));
        Assert.That(invitation.Type, Is.EqualTo(ClanInvitationType.Offer));
        Assert.That(invitation.Status, Is.EqualTo(ClanInvitationStatus.Pending));
    }
}
