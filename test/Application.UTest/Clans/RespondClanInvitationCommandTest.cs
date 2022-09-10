using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class RespondClanInvitationCommandTest : TestBase
{
    private static readonly IClanService ClanService = new ClanService();

    [Test]
    public async Task ShouldReturnErrorIfUserNotFound()
    {
        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = 1,
            ClanId = 2,
            ClanInvitationId = 3,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfClanInvitationNotFound()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            ClanInvitationId = 3,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanInvitationNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfClanIdAndClanIdOfInvitationDontMatch()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan1 = new();
        Clan clan2 = new();
        ArrangeDb.Clans.AddRange(clan1, clan2);
        ClanInvitation clanInvitation = new() { Clan = clan2 };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clan1.Id,
            ClanInvitationId = clanInvitation.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanInvitationNotFound, res.Errors![0].Code);
    }

    [Theory]
    public async Task ShouldReturnErrorIfOfferButUserNotTheInvitee(bool accept)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = new User(),
            Inviter = new User(),
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
    }

    [Theory]
    public async Task ShouldReturnErrorIfOfferButUserIsTheInviter(bool accept)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = new User(),
            Inviter = user,
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
    }

    [TestCase(false, ClanInvitationStatus.Declined)]
    [TestCase(true, ClanInvitationStatus.Declined)]
    [TestCase(false, ClanInvitationStatus.Accepted)]
    [TestCase(true, ClanInvitationStatus.Accepted)]
    public async Task ShouldReturnErrorIfOfferButInvitationIsClosed(bool accept, ClanInvitationStatus status)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = new User(),
            Type = ClanInvitationType.Offer,
            Status = status,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
    }

    [Test]
    public async Task DecliningInvitationShouldWork()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = new User(),
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = false,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(clanInvitation.Id, res.Data!.Id);
        Assert.AreEqual(ClanInvitationStatus.Declined, res.Data!.Status);
        Assert.AreNotEqual(user.Id, res.Data!.Inviter.Id);
    }

    [Test]
    public async Task AcceptingInvitationShouldWorkIfUserIsInAClan()
    {
        User user = new() { ClanMembership = new ClanMember { Role = ClanMemberRole.Member, Clan = new Clan() } };
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = new User(),
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(clanInvitation.Id, res.Data!.Id);
        Assert.AreEqual(ClanInvitationStatus.Accepted, res.Data!.Status);
        Assert.AreNotEqual(user.Id, res.Data!.Inviter.Id);
        Assert.That(AssertDb.ClanMembers, Has.Exactly(1)
            .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
    }

    [Test]
    public async Task AcceptingInvitationShouldWorkIfUserIsNotInAClan()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = new User(),
            Type = ClanInvitationType.Offer,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(clanInvitation.Id, res.Data!.Id);
        Assert.AreEqual(ClanInvitationStatus.Accepted, res.Data!.Status);
        Assert.AreNotEqual(user.Id, res.Data!.Inviter.Id);
        Assert.That(AssertDb.ClanMembers, Has.Exactly(1)
            .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
    }

    [Theory]
    public async Task ShouldReturnErrorIfRequestButUserIsTheInviter(bool accept)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
    }

    [Theory]
    public async Task ShouldReturnErrorIfRequestButUserNotInTheClan(bool accept)
    {
        User user = new() { ClanMembership = new ClanMember { Role = ClanMemberRole.Member, Clan = new Clan() } };
        ArrangeDb.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotAClanMember, res.Errors![0].Code);
    }

    [Theory]
    public async Task ShouldReturnErrorIfRequestButUserNotAClanOfficer(bool accept)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new() { Members = { new ClanMember { Role = ClanMemberRole.Member, User = user } } };
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
    }

    [TestCase(false, ClanInvitationStatus.Declined)]
    [TestCase(true, ClanInvitationStatus.Declined)]
    [TestCase(false, ClanInvitationStatus.Accepted)]
    [TestCase(true, ClanInvitationStatus.Accepted)]
    public async Task ShouldReturnErrorIfRequestButInvitationIsClosed(bool accept, ClanInvitationStatus status)
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new() { Members = { new ClanMember { Role = ClanMemberRole.Officer, User = user } } };
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = status,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = accept,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanInvitationClosed, res.Errors![0].Code);
    }

    [Test]
    public async Task AcceptingRequestShouldWork()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new() { Members = { new ClanMember { Role = ClanMemberRole.Officer, User = user } } };
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(clanInvitation.Id, res.Data!.Id);
        Assert.AreEqual(ClanInvitationStatus.Accepted, res.Data!.Status);
        Assert.AreEqual(user.Id, res.Data!.Inviter.Id);
    }

    [Test]
    public async Task DecliningRequestShouldWork()
    {
        User user = new();
        ArrangeDb.Add(user);
        Clan clan = new() { Members = { new ClanMember { Role = ClanMemberRole.Officer, User = user } } };
        ArrangeDb.Clans.Add(clan);
        ClanInvitation clanInvitation = new()
        {
            Clan = clan,
            Invitee = user,
            Inviter = user,
            Type = ClanInvitationType.Request,
            Status = ClanInvitationStatus.Pending,
        };
        ArrangeDb.ClanInvitations.Add(clanInvitation);
        await ArrangeDb.SaveChangesAsync();

        var res = await new RespondClanInvitationCommand.Handler(ActDb, Mapper, ClanService).Handle(new RespondClanInvitationCommand
        {
            UserId = user.Id,
            ClanId = clanInvitation.ClanId,
            ClanInvitationId = clanInvitation.Id,
            Accept = false,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(clanInvitation.Id, res.Data!.Id);
        Assert.AreEqual(ClanInvitationStatus.Declined, res.Data!.Status);
        Assert.AreEqual(user.Id, res.Data!.Inviter.Id);
    }
}
