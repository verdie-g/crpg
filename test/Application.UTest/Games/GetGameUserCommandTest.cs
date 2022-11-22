using Crpg.Application.Common.Files;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
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
            .GroupBy(i => i.id) // distinct by mbId
            .Select(p => new Item { Id = p.First().id });

        ArrangeDb.Items.AddRange(allDefaultItemMbIds);
        await ArrangeDb.SaveChangesAsync();
    }

    [Test]
    public async Task ShouldCreateUserIfDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, new MachineDateTime(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = Platform.Epic,
            PlatformUserId = "1",
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.NotZero(gameUser.Id);
        Assert.AreEqual(Platform.Epic, gameUser.Platform);
        Assert.AreEqual("1", gameUser.PlatformUserId);
        Assert.AreEqual("Peasant", gameUser.Character.Name);
        Assert.IsNotEmpty(gameUser.Character.EquippedItems);
        Assert.IsEmpty(gameUser.Restrictions);

        // Check that default values were set for user and character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()));
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), false));

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

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Gold = 1000,
            Characters =
            {
                new Character { CreatedAt = DateTime.UtcNow.AddHours(-2) },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, new MachineDateTime(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.IsNull(result.Errors);
        Assert.AreEqual(user.Id, gameUser.Id);
        Assert.AreEqual(user.Platform, gameUser.Platform);
        Assert.AreEqual(user.PlatformUserId, gameUser.PlatformUserId);
        Assert.AreEqual("Peasant", gameUser.Character.Name);
        Assert.IsNotEmpty(gameUser.Character.EquippedItems);
        Assert.IsEmpty(gameUser.Restrictions);

        // Check that default values were set for character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()), Times.Never);
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), false));

        // Check that user and its owned entities were created
        var dbUser = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
            .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

        Assert.IsNotNull(dbUser);
        Assert.IsNotEmpty(dbUser!.Characters);
        Assert.IsNotEmpty(dbUser.Characters[1].EquippedItems);
    }

    [Theory]
    public async Task ShouldNotCreateCharacterIfOneWasCreatedRecently(bool characterDeleted)
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Gold = 1000,
            Characters =
            {
                new Character
                {
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    DeletedAt = characterDeleted ? DateTime.UtcNow.AddMinutes(-3) : null,
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, new MachineDateTime(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object);

        var res = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.CharacterRecentlyCreated, res.Errors![0].Code);
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
                new UserItem { BaseItemId = ArrangeDb.Items.First(i => i.Id == GetGameUserCommand.Handler.DefaultItemSets[1][0].id).Id },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        // Make sure to always give the same item set to the character
        Mock<IRandom> randomMock = new();
        randomMock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(1);

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            new MachineDateTime(), randomMock.Object, userService, characterService);

        // Handle shouldn't throw
        await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var userItems = await AssertDb.UserItems.Where(oi => oi.UserId == user.Id).ToArrayAsync();
        Assert.AreEqual(5, userItems.Length);
    }

    [Test]
    public async Task ShouldGetWithPlatformUserAndPlatform()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        Character user0Character = new();
        User user0 = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            ActiveCharacter = user0Character,
            Characters = { user0Character },
        };

        Character user1Character = new();
        User user1 = new()
        {
            Platform = Platform.Epic,
            PlatformUserId = user0.PlatformUserId, // Same platform user id but different platform
            ActiveCharacter = user1Character,
            Characters = { user1Character },
        };

        ArrangeDb.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            new MachineDateTime(), new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user0.Platform,
            PlatformUserId = user0.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.AreEqual(user0.Platform, gameUser.Platform);
        Assert.AreEqual(user0.PlatformUserId, gameUser.PlatformUserId);
        Assert.AreEqual(user0Character.Id, gameUser.Character.Id);
    }

    [Test]
    public async Task ShouldGetActiveCharacterWhenSeveralExists()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        Character character = new();
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            ActiveCharacter = character,
            Characters = new List<Character>
            {
                character,
                new(),
                new(),
            },
        };
        ArrangeDb.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            new MachineDateTime(), new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.AreEqual(character.Id, gameUser.Character.Id);
    }

    [Test]
    public async Task RestrictionsShouldntBeEmptyForRestrictedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "1",
            Restrictions = new List<Restriction>
            {
                new()
                {
                    Type = RestrictionType.Chat,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(1999, 1, 1),
                },
                new()
                {
                    Type = RestrictionType.Chat,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(2000, 1, 1),
                },
                new()
                {
                    Type = RestrictionType.Join,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(1999, 1, 1),
                },
                new()
                {
                    Type = RestrictionType.Join,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(2000, 1, 1),
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTime = new();
        dateTime
            .Setup(dt => dt.UtcNow)
            .Returns(new DateTime(2000, 1, 1, 12, 0, 0));

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            dateTime.Object, new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gamerUser = result.Data!;
        Assert.AreEqual(2, gamerUser.Restrictions.Count);
    }

    [Test]
    public async Task RestrictionsShouldBeEmptyForUnrestrictedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();

        User user = new()
        {
            PlatformUserId = "1",
            Restrictions = new List<Restriction>
            {
                new()
                {
                    Type = RestrictionType.Join,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(2000, 1, 1),
                },
                new()
                {
                    Type = RestrictionType.Join,
                    Duration = TimeSpan.Zero,
                    CreatedAt = new DateTime(2000, 1, 1, 6, 0, 0),
                },
                new()
                {
                    Type = RestrictionType.Chat,
                    Duration = TimeSpan.FromDays(1),
                    CreatedAt = new DateTime(2000, 1, 1),
                },
                new()
                {
                    Type = RestrictionType.Chat,
                    Duration = TimeSpan.Zero,
                    CreatedAt = new DateTime(2000, 1, 1, 6, 0, 0),
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTime = new();
        dateTime
            .Setup(dt => dt.UtcNow)
            .Returns(new DateTime(2000, 1, 1, 12, 0, 0));

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            dateTime.Object, new ThreadSafeRandom(), userService, characterService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.IsEmpty(gameUser.Restrictions);
    }

    [Test]
    public async Task CheckDefaultItemsExist()
    {
        var items = (await new FileItemsSource().LoadItems()).ToDictionary(i => i.Id);
        foreach (var set in GetGameUserCommand.Handler.DefaultItemSets)
        {
            foreach ((string mbId, ItemSlot slot) in set)
            {
                Assert.IsTrue(items.ContainsKey(mbId), $"Item '{mbId}' doesn't exist");
            }
        }
    }
}
