using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class UpdateClanMemberCommandTest : TestBase
{
    private static readonly IClanService ClanService = new ClanService();

    [TestCase(ClanMemberRole.Member)]
    [TestCase(ClanMemberRole.Officer)]
    public async Task ShouldReturnErrorIfUserNotLeader(ClanMemberRole userRole)
    {
        Clan clan = new();
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
        User member = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        ArrangeDb.Users.AddRange(user, member);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            MemberId = member.Id,
            Role = ClanMemberRole.Officer,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
    }

    [Test]
    public async Task ShouldReturnErrorIfLeaderChangesItsRole()
    {
        Clan clan = new();
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            MemberId = user.Id,
            Role = ClanMemberRole.Officer,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
    }

    [Test]
    public async Task ShouldUpdateMember()
    {
        Clan clan = new();
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        User member = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        ArrangeDb.Users.AddRange(user, member);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            MemberId = member.Id,
            Role = ClanMemberRole.Officer,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var memberVm = res.Data!;
        Assert.That(memberVm.User.Id, Is.EqualTo(member.Id));
        Assert.That(memberVm.Role, Is.EqualTo(ClanMemberRole.Officer));
    }

    [Test]
    public async Task ShouldGiveLeaderRole()
    {
        Clan clan = new();
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        User member = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
        ArrangeDb.Users.AddRange(user, member);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            MemberId = member.Id,
            Role = ClanMemberRole.Leader,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var memberVm = res.Data!;
        Assert.That(memberVm.User.Id, Is.EqualTo(member.Id));
        Assert.That(memberVm.Role, Is.EqualTo(ClanMemberRole.Leader));
        var userMember = await AssertDb.ClanMembers.FirstAsync(m => m.UserId == user.Id);
        Assert.That(userMember.Role, Is.EqualTo(ClanMemberRole.Officer));
    }
}
