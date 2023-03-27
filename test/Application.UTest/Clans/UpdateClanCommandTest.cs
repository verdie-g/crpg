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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanMemberRoleNotMet));
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
            Region = Region.Na,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ClanTagAlreadyUsed));
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
            Region = Region.Na,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNameAlreadyUsed));
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
            Description = "C",
            BannerKey = "789",
            Region = Region.As,
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
            Description = "E",
            BannerKey = "7890",
            Region = Region.Na,
            Discord = new Uri("https://discord.gg/def"),
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var clanVm = res.Data!;
        Assert.That(clanVm.Region, Is.EqualTo(Region.Na));
        Assert.That(clanVm.Tag, Is.EqualTo("C"));
        Assert.That(clanVm.Description, Is.EqualTo("E"));
        Assert.That(clanVm.PrimaryColor, Is.EqualTo(1234));
        Assert.That(clanVm.SecondaryColor, Is.EqualTo(4567));
        Assert.That(clanVm.Name, Is.EqualTo("D"));
        Assert.That(clanVm.BannerKey, Is.EqualTo("7890"));
        Assert.That(clanVm.Region, Is.EqualTo(Region.Na));
        Assert.That(clanVm.Discord, Is.EqualTo(new Uri("https://discord.gg/def")));
    }
}
