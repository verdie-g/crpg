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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanInvitationNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanInvitationNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
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

        Assert.That(res.Errors, Is.Not.Null);
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

        Assert.That(res.Errors, Is.Not.Null);
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(clanInvitation.Id));
        Assert.That(res.Data!.Status, Is.EqualTo(ClanInvitationStatus.Declined));
        Assert.That(res.Data!.Inviter.Id, Is.Not.EqualTo(user.Id));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(clanInvitation.Id));
        Assert.That(res.Data!.Status, Is.EqualTo(ClanInvitationStatus.Accepted));
        Assert.That(res.Data!.Inviter.Id, Is.Not.EqualTo(user.Id));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(clanInvitation.Id));
        Assert.That(res.Data!.Status, Is.EqualTo(ClanInvitationStatus.Accepted));
        Assert.That(res.Data!.Inviter.Id, Is.Not.EqualTo(user.Id));
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

        Assert.That(res.Errors, Is.Not.Null);
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotAClanMember));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanInvitationClosed));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(clanInvitation.Id));
        Assert.That(res.Data!.Status, Is.EqualTo(ClanInvitationStatus.Accepted));
        Assert.That(res.Data!.Inviter.Id, Is.EqualTo(user.Id));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(clanInvitation.Id));
        Assert.That(res.Data!.Status, Is.EqualTo(ClanInvitationStatus.Declined));
        Assert.That(res.Data!.Inviter.Id, Is.EqualTo(user.Id));
    }
}
