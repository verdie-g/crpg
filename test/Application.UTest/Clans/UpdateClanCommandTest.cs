using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class UpdateClanCommandTest : TestBase
{
    private static readonly IClanService ClanService = new ClanService();

    [Test]
    public async Task ShouldReturnErrorIfClanNotFound()
    {
        Clan clan = new();
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanCommand
        {
            UserId = user.Id,
            ClanId = 234,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ClanNotFound, res.Errors![0].Code);
    }

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
    public async Task ShouldReturnErrorIfTagAlreadyExists()
    {
        User user = new();
        ArrangeDb.Add(user);
        ArrangeDb.Clans.Add(new Clan { Tag = "TW" });
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
        {
            UserId = user.Id,
            Tag = "TW",
            PrimaryColor = 0xFFFFFFFF,
            SecondaryColor = 0xFF000000,
            Name = "TaleWorlds",
            BannerKey = string.Empty,
            Region = Region.NorthAmerica,
        }, CancellationToken.None);

        Assert.NotNull(result.Errors);
        Assert.IsNotEmpty(result.Errors!);
        Assert.AreEqual(ErrorCode.ClanTagAlreadyUsed, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfNameAlreadyExists()
    {
        User user = new();
        ArrangeDb.Add(user);
        ArrangeDb.Clans.Add(new Clan { Name = "TaleWorlds" });
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
        {
            UserId = user.Id,
            Tag = "TW",
            PrimaryColor = 0xFFFFFFFF,
            SecondaryColor = 0xFF000000,
            Name = "TaleWorlds",
            BannerKey = string.Empty,
            Region = Region.NorthAmerica,
        }, CancellationToken.None);

        Assert.NotNull(result.Errors);
        Assert.IsNotEmpty(result.Errors!);
        Assert.AreEqual(ErrorCode.ClanNameAlreadyUsed, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldUpdateClan()
    {
        Clan clan = new()
        {
            Tag = "A",
            PrimaryColor = 123,
            SecondaryColor = 456,
            Name = "B",
            BannerKey = "789",
            Region = Region.Asia,
            Discord = new Uri("https://discord.gg/abc"),
        };
        User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateClanCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanCommand
        {
            UserId = user.Id,
            ClanId = clan.Id,
            Tag = "C",
            PrimaryColor = 1234,
            SecondaryColor = 4567,
            Name = "D",
            BannerKey = "7890",
            Region = Region.NorthAmerica,
            Discord = new Uri("https://discord.gg/def"),
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var clanVm = res.Data!;
        Assert.AreEqual(Region.NorthAmerica, clanVm.Region);
        Assert.AreEqual("C", clanVm.Tag);
        Assert.AreEqual(1234, clanVm.PrimaryColor);
        Assert.AreEqual(4567, clanVm.SecondaryColor);
        Assert.AreEqual("D", clanVm.Name);
        Assert.AreEqual("7890", clanVm.BannerKey);
        Assert.AreEqual(Region.NorthAmerica, clanVm.Region);
        Assert.AreEqual(new Uri("https://discord.gg/def"), clanVm.Discord);
    }
}
