using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class CreateClanCommandTest : TestBase
{
    [Test]
    public async Task ShouldCreateClan()
    {
        User user = new();
        ArrangeDb.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
        {
            UserId = user.Id,
            Tag = "TW",
            PrimaryColor = 0xFFFFFFFF,
            SecondaryColor = 0xFF000000,
            Name = "TaleWorlds",
            BannerKey = "abc",
            Region = Region.NorthAmerica,
            Discord = new Uri("https://discord.gg/abc"),
        }, CancellationToken.None);

        var clan = result.Data!;
        Assert.AreEqual("TW", clan.Tag);
        Assert.AreEqual(0xFFFFFFFF, clan.PrimaryColor);
        Assert.AreEqual(0xFF000000, clan.SecondaryColor);
        Assert.AreEqual("TaleWorlds", clan.Name);
        Assert.AreEqual("abc", clan.BannerKey);
        Assert.AreEqual(Region.NorthAmerica, clan.Region);
        Assert.AreEqual(new Uri("https://discord.gg/abc"), clan.Discord);

        Assert.That(AssertDb.Clans, Has.Exactly(1).Matches<Clan>(c => c.Id == clan.Id));
        Assert.That(AssertDb.ClanMembers, Has.Exactly(1)
            .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
    }

    [Test]
    public async Task ShouldReturnErrorIfUserDoesntExist()
    {
        var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
        {
            UserId = 1,
            Tag = "TW",
            PrimaryColor = 0xFFFFFFFF,
            SecondaryColor = 0xFF000000,
            Name = "TaleWorlds",
            BannerKey = string.Empty,
        }, CancellationToken.None);

        Assert.NotNull(result.Errors);
        Assert.IsNotEmpty(result.Errors!);
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfUserIsAlreadyInAClan()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
        {
            UserId = user.Id,
            Tag = "TW",
            PrimaryColor = 0xFFFFFFFF,
            SecondaryColor = 0xFF000000,
            Name = "TaleWorlds",
            BannerKey = string.Empty,
        }, CancellationToken.None);

        Assert.NotNull(result.Errors);
        Assert.IsNotEmpty(result.Errors!);
        Assert.AreEqual(ErrorCode.UserAlreadyInAClan, result.Errors![0].Code);
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
        }, CancellationToken.None);

        Assert.NotNull(result.Errors);
        Assert.IsNotEmpty(result.Errors!);
        Assert.AreEqual(ErrorCode.ClanNameAlreadyUsed, result.Errors![0].Code);
    }
}
