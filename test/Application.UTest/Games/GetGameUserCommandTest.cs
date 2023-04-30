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
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        GetGameUserCommand.Handler handler = new(ActDb, Mapper, new MachineDateTime(),
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object, activityLogServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = Platform.EpicGames,
            PlatformUserId = "1",
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.That(gameUser.Id, Is.Not.Zero);
        Assert.That(gameUser.Platform, Is.EqualTo(Platform.EpicGames));
        Assert.That(gameUser.PlatformUserId, Is.EqualTo("1"));
        Assert.That(gameUser.Character.Name, Is.EqualTo("Peasant"));
        Assert.That(gameUser.Character.Class, Is.EqualTo(CharacterClass.Peasant));
        Assert.That(gameUser.Character.EquippedItems, Is.Not.Empty);
        Assert.That(gameUser.Restrictions, Is.Empty);

        // Check that default values were set for user and character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()));
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));

        // Check that user and its owned entities were created
        var dbUser = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
            .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Characters, Is.Not.Empty);
        Assert.That(dbUser.Characters[0].EquippedItems, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldCreateCharacterIfDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

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
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object, activityLogServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.That(result.Errors, Is.Null);
        Assert.That(gameUser.Id, Is.EqualTo(user.Id));
        Assert.That(gameUser.Platform, Is.EqualTo(user.Platform));
        Assert.That(gameUser.PlatformUserId, Is.EqualTo(user.PlatformUserId));
        Assert.That(gameUser.Character.Name, Is.EqualTo("Peasant"));
        Assert.That(gameUser.Character.Class, Is.EqualTo(CharacterClass.Peasant));
        Assert.That(gameUser.Character.EquippedItems, Is.Not.Empty);
        Assert.That(gameUser.Restrictions, Is.Empty);

        // Check that default values were set for character.
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()), Times.Never);
        characterServiceMock.Verify(cs => cs.SetDefaultValuesForCharacter(It.IsAny<Character>()));

        // Check that user and its owned entities were created
        var dbUser = await AssertDb.Users
            .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
            .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.Characters, Is.Not.Empty);
        Assert.That(dbUser.Characters[1].EquippedItems, Is.Not.Empty);
    }

    [Theory]
    public async Task ShouldNotCreateCharacterIfOneWasCreatedRecently(bool characterDeleted)
    {
        Mock<IUserService> userServiceMock = new();
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

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
            new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object, activityLogServiceMock.Object);

        var res = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterRecentlyCreated));
    }

    [Test]
    public async Task ShouldNotAddUserItemWhenCreatingCharacterIfItemIsAlreadyOwned()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

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
            new MachineDateTime(), randomMock.Object, userService, characterService, activityLogServiceMock.Object);

        // Handle shouldn't throw
        await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var userItems = await AssertDb.UserItems.Where(oi => oi.UserId == user.Id).ToArrayAsync();
        Assert.That(userItems.Length, Is.EqualTo(5));
    }

    [Test]
    public async Task ShouldGetWithPlatformUserAndPlatform()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();

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
            Platform = Platform.EpicGames,
            PlatformUserId = user0.PlatformUserId, // Same platform user id but different platform
            ActiveCharacter = user1Character,
            Characters = { user1Character },
        };

        ArrangeDb.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCommand.Handler handler = new(ActDb, Mapper,
            new MachineDateTime(), new ThreadSafeRandom(), userService, characterService, activityLogService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user0.Platform,
            PlatformUserId = user0.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.That(gameUser.Platform, Is.EqualTo(user0.Platform));
        Assert.That(gameUser.PlatformUserId, Is.EqualTo(user0.PlatformUserId));
        Assert.That(gameUser.Character.Id, Is.EqualTo(user0Character.Id));
    }

    [Test]
    public async Task ShouldGetActiveCharacterWhenSeveralExists()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();

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
            new MachineDateTime(), new ThreadSafeRandom(), userService, characterService, activityLogService);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.That(gameUser.Character.Id, Is.EqualTo(character.Id));
    }

    [Test]
    public async Task RestrictionsShouldntBeEmptyForRestrictedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

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
            dateTime.Object, new ThreadSafeRandom(), userService, characterService, activityLogServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gamerUser = result.Data!;
        Assert.That(gamerUser.Restrictions.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task RestrictionsShouldBeEmptyForUnrestrictedUser()
    {
        var userService = Mock.Of<IUserService>();
        var characterService = Mock.Of<ICharacterService>();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

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
            dateTime.Object, new ThreadSafeRandom(), userService, characterService, activityLogServiceMock.Object);

        var result = await handler.Handle(new GetGameUserCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = result.Data!;
        Assert.That(gameUser.Restrictions, Is.Empty);
    }

    [Test]
    public async Task CheckDefaultItemsExistAndAreNotTooExpensive()
    {
        var items = (await new FileItemsSource().LoadItems()).ToDictionary(i => i.Id);
        foreach (var set in GetGameUserCommand.Handler.DefaultItemSets)
        {
            int price = 0;
            foreach ((string mbId, ItemSlot slot) in set)
            {
                if (!items.TryGetValue(mbId, out var item))
                {
                    Assert.That(items, Does.ContainKey(mbId), $"Item '{mbId}' doesn't exist");
                    continue;
                }

                price += item.Price;
            }

            Assert.That(price, Is.LessThan(2900));
        }
    }
}
