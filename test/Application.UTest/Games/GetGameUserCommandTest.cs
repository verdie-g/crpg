using Crpg.Application.Common.Services;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;

public class GetGameUserCommandTest : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();

        var allDefaultItemMbIds = GetGameUserCommand.Handler.DefaultItemSets
            .SelectMany(set => set)
            .GroupBy(i => i.mbId) // distinct by mbId
            .Select(p => new Item { TemplateMbId = p.First().mbId });

        ArrangeDb.Items.AddRange(allDefaultItemMbIds);
        await ArrangeDb.SaveChangesAsync();
    }

    [Test]
    public async Task ShouldCreateUserIfDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(), new MachineDateTimeOffset(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = Platform.Epic,
            PlatformUserId = "1",
            UserName = "a",
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.NotZero(gameUser.Id);
        Assert.AreEqual(Platform.Epic, gameUser.Platform);
        Assert.AreEqual("1", gameUser.PlatformUserId);
        Assert.AreEqual("a", gameUser.Character.Name);
        Assert.IsNotEmpty(gameUser.Character.EquippedItems);
        Assert.IsNull(gameUser.Ban);

        // Check that default values were set for user and character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()));
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));
        characterServiceMock.Verify(cs => cs.ResetCharacterStats(It.IsAny<Character>(), false));

        // Check that user and its owned entities were created
        var dbUser = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
            .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

        Assert.IsNotNull(dbUser);
        Assert.IsNotEmpty(dbUser!.Characters);
        Assert.IsNotEmpty(dbUser.Characters[0].EquippedItems);
    }

    [Test]
    public async Task ShouldCreateCharacterIfDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        User user = new() { Platform = Platform.Steam, PlatformUserId = "1", Gold = 1000 };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(), new MachineDateTimeOffset(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
            UserName = "a",
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.AreEqual(user.Id, gameUser.Id);
        Assert.AreEqual(user.Platform, gameUser.Platform);
        Assert.AreEqual(user.PlatformUserId, gameUser.PlatformUserId);
        Assert.AreEqual("a", gameUser.Character.Name);
        Assert.IsNotEmpty(gameUser.Character.EquippedItems);
        Assert.IsNull(gameUser.Ban);

        // Check that default values were set for character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()), Times.Never);
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));
        characterServiceMock.Verify(cs => cs.ResetCharacterStats(It.IsAny<Character>(), false));

        // Check that user and its owned entities were created
        var dbUser = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
            .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

        Assert.IsNotNull(dbUser);
        Assert.IsNotEmpty(dbUser!.Characters);
        Assert.IsNotEmpty(dbUser.Characters[0].EquippedItems);
    }

    [Test]
    public async Task ShouldNotAddUserItemWhenCreatingCharacterIfItemIsAlreadyOwned()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Items =
            {
                // Already owned item
                new UserItem { ItemId = ArrangeDb.Items.First(i => i.TemplateMbId == GetGameUserCommand.Handler.DefaultItemSets[1][0].mbId).Id },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        // Make sure to always give the same item set to the character
        Mock<IRandom> randomMock = new();
        randomMock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(1);

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(),
            new MachineDateTimeOffset(), randomMock.Object, userService, characterService);

        // Handle shouldn't throw
        await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
            UserName = "a",
        }, CancellationToken.None);

        var userItems = await AssertDb.UserItems.Where(oi => oi.UserId == user.Id).ToArrayAsync();
        Assert.AreEqual(5, userItems.Length);
    }

    [Test]
    public async Task ShouldGetWithPlatformUserAndPlatform()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user0 = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Characters = new List<Character> { new() { Name = "a" } },
        };

        User user1 = new()
        {
            Platform = Platform.Epic,
            PlatformUserId = user0.PlatformUserId, // Same platform user id but different platform
            Characters = new List<Character> { new() { Name = "a" } },
        };

        ArrangeDb.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(),
            new MachineDateTimeOffset(), new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user0.Platform,
            PlatformUserId = user0.PlatformUserId,
            UserName = user0.Characters[0].Name,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.AreEqual(user0.Platform, gameUser.Platform);
        Assert.AreEqual(user0.PlatformUserId, gameUser.PlatformUserId);
        Assert.AreEqual(user0.Characters[0].Name, gameUser.Character.Name);
    }

    [Test]
    public async Task ShouldGetSpecifiedCharacterWhenSeveralExists()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Characters = new List<Character>
            {
                new() { Name = "a" },
                new() { Name = "b" },
                new() { Name = "c" },
            },
        };
        ArrangeDb.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(),
            new MachineDateTimeOffset(), new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
            UserName = user.Characters[1].Name,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.AreEqual(user.Characters[1].Id, gameUser.Character.Id);
    }

    [Test]
    public async Task BanShouldntBeNullForBannedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Bans = new List<Ban>
            {
                new()
                {
                    CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                    Duration = TimeSpan.FromDays(1),
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTimeOffset> dateTime = new();
        dateTime
            .Setup(dt => dt.Now)
            .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(),
            dateTime.Object, new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
            UserName = "a",
        }, CancellationToken.None);

        var gamerUser = result.Data!;
        Assert.NotNull(gamerUser.Ban);
    }

    [Test]
    public async Task BanShouldBeNullForUnbannedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            PlatformUserId = "1",
            Bans = new List<Ban>
            {
                new()
                {
                    CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                    Duration = TimeSpan.FromDays(1),
                },
                new()
                {
                    CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1, 6, 0, 0)),
                    Duration = TimeSpan.Zero,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTimeOffset> dateTime = new();
        dateTime
            .Setup(dt => dt.Now)
            .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IEventService>(),
            dateTime.Object, new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
            UserName = "a",
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.Null(gameUser.Ban);
    }
}
