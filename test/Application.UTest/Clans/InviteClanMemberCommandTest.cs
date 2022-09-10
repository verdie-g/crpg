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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserAlreadyInTheClan, res.Errors![0].Code);
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
        Assert.IsNull(res.Errors);
        Assert.NotZero(invitation.Id);
        Assert.AreEqual(user.Id, invitation.Invitee.Id);
        Assert.AreEqual(user.Id, invitation.Inviter.Id);
        Assert.AreEqual(ClanInvitationType.Request, invitation.Type);
        Assert.AreEqual(ClanInvitationStatus.Pending, invitation.Status);
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

        Assert.IsNull(res.Errors);
        Assert.AreEqual(invitation.Id, res.Data!.Id);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserAlreadyInTheClan, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotInAClan, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotAClanMember, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
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

        Assert.IsNull(res.Errors);
        Assert.AreEqual(offer.Id, res.Data!.Id);
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
        Assert.IsNull(res.Errors);
        Assert.NotZero(invitation.Id);
        Assert.AreEqual(invitee.Id, invitation.Invitee.Id);
        Assert.AreEqual(inviter.Id, invitation.Inviter.Id);
        Assert.AreEqual(ClanInvitationType.Offer, invitation.Type);
        Assert.AreEqual(ClanInvitationStatus.Pending, invitation.Status);
    }
}
