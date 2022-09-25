using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class UpdateClanCommandTest : TestBase
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

        var res = await new UpdateClanCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfClanNotFound()
    {
        Clan clan = new() { };
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanCommand
        {
            UserId = user.Id,
            ClanId = 234
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldUpdateClan()
    {
        Clan clan = new() { Region = Domain.Entities.Region.Asia };
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            Region = Domain.Entities.Region.NorthAmerica
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var clanVm = res.Data!;
        Assert.AreEqual(clanVm.Region, Domain.Entities.Region.NorthAmerica);
    }

}
