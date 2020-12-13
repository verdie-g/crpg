using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
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
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class GetGameUserCommandTest : TestBase
    {
        private static readonly ILogger<GetGameUserCommand> Logger = Mock.Of<ILogger<GetGameUserCommand>>();

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
            var userServiceMock = new Mock<IUserService>();
            var characterServiceMock = new Mock<ICharacterService>();

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(), new MachineDateTimeOffset(),
                new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object, Logger);

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
            Assert.IsNotEmpty(dbUser.Characters);
            Assert.IsNotEmpty(dbUser.Characters[0].EquippedItems);
        }

        [Test]
        public async Task ShouldCreateCharacterIfDoesntExist()
        {
            var userServiceMock = new Mock<IUserService>();
            var characterServiceMock = new Mock<ICharacterService>();

            var user = new User { Platform = Platform.Steam, PlatformUserId = "1", Gold = 1000 };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(), new MachineDateTimeOffset(),
                new ThreadSafeRandom(), userServiceMock.Object, characterServiceMock.Object, Logger);

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
            Assert.IsNotEmpty(dbUser.Characters);
            Assert.IsNotEmpty(dbUser.Characters[0].EquippedItems);
        }

        [Test]
        public async Task ShouldNotAddOwnedItemWhenCreatingCharacterIfItemIsAlreadyOwned()
        {
            var userService = Mock.Of<IUserService>();
            var characterService = Mock.Of<ICharacterService>();

            var user = new User
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                OwnedItems =
                {
                    // Already owned item
                    new OwnedItem { ItemId = ArrangeDb.Items.First(i => i.TemplateMbId == GetGameUserCommand.Handler.DefaultItemSets[1][0].mbId).Id },
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            // Make sure to always give the same item set to the character
            var randomMock = new Mock<IRandom>();
            randomMock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(1);

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), randomMock.Object, userService, characterService, Logger);

            // Handle shouldn't throw
            await handler.Handle(new GetGameUserCommand
            {
                Platform = user.Platform,
                PlatformUserId = user.PlatformUserId,
                UserName = "a",
            }, CancellationToken.None);

            var ownedItems = await AssertDb.OwnedItems.Where(oi => oi.UserId == user.Id).ToArrayAsync();
            Assert.AreEqual(5, ownedItems.Length);
        }

        [Test]
        public async Task ShouldGetWithPlatformUserAndPlatform()
        {
            var userService = Mock.Of<IUserService>();
            var characterService = Mock.Of<ICharacterService>();

            var user0 = new User
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                Characters = new List<Character> { new Character { Name = "a" } },
            };

            var user1 = new User
            {
                Platform = Platform.Epic,
                PlatformUserId = user0.PlatformUserId, // Same platform user id but different platform
                Characters = new List<Character> { new Character { Name = "a" } },
            };

            ArrangeDb.AddRange(user0, user1);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom(), userService, characterService, Logger);

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

            var user = new User
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                Characters = new List<Character>
                {
                    new Character { Name = "a" },
                    new Character { Name = "b" },
                    new Character { Name = "c" },
                },
            };
            ArrangeDb.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom(), userService, characterService, Logger);

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

            var user = new User
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                Bans = new List<Ban>
                {
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                        Duration = TimeSpan.FromDays(1),
                    }
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime
                .Setup(dt => dt.Now)
                .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                dateTime.Object, new ThreadSafeRandom(), userService, characterService, Logger);

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

            var user = new User
            {
                PlatformUserId = "1",
                Bans = new List<Ban>
                {
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1)),
                        Duration = TimeSpan.FromDays(1),
                    },
                    new Ban
                    {
                        CreatedAt = new DateTimeOffset(new DateTime(2000, 1, 1, 6, 0, 0)),
                        Duration = TimeSpan.Zero,
                    }
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime
                .Setup(dt => dt.Now)
                .Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 12, 0, 0)));

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                dateTime.Object, new ThreadSafeRandom(), userService, characterService, Logger);

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
}
