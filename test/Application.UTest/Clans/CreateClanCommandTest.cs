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
            Description = "a description",
            BannerKey = "abc",
            Region = Region.Na,
            Discord = new Uri("https://discord.gg/abc"),
        }, CancellationToken.None);

        var clan = result.Data!;
        Assert.That(clan.Tag, Is.EqualTo("TW"));
        Assert.That(clan.PrimaryColor, Is.EqualTo(0xFFFFFFFF));
        Assert.That(clan.SecondaryColor, Is.EqualTo(0xFF000000));
        Assert.That(clan.Name, Is.EqualTo("TaleWorlds"));
        Assert.That(clan.Description, Is.EqualTo("a description"));
        Assert.That(clan.BannerKey, Is.EqualTo("abc"));
        Assert.That(clan.Region, Is.EqualTo(Region.Na));
        Assert.That(clan.Discord, Is.EqualTo(new Uri("https://discord.gg/abc")));

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

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
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

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserAlreadyInAClan));
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
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNameAlreadyUsed));
    }
}
