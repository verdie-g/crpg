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

        private static readonly Constants Constants = new Constants
        {
            DefaultGeneration = 0,
            MinimumLevel = 1,
            DefaultExperienceMultiplier = 1f,
            DefaultAutoRepair = true,
            DefaultCharacterBodyProperties = "ABCD",
            DefaultCharacterGender = CharacterGender.Male,
            DefaultGold = 300,
            DefaultRole = Role.User,
            DefaultHeirloomPoints = 0,
            DefaultStrength = 3,
            DefaultAgility = 3,
            WeaponProficiencyPointsForLevelCoefs = new[] { 100f, 0f }, // wpp = lvl * 100
        };

        private static readonly UserService UserService = new UserService(Constants);
        private static readonly CharacterService CharacterService = new CharacterService(null!, Constants);

        [SetUp]
        public override async Task SetUp()
        {
            await base.SetUp();

            var allDefaultItemMbIds = GetGameUserCommand.Handler.DefaultItemSets
                .SelectMany(set => set)
                .GroupBy(i => i.mbId) // distinct by mbId
                .Select(p => new Item { MbId = p.First().mbId });

            ArrangeDb.Items.AddRange(allDefaultItemMbIds);
            await ArrangeDb.SaveChangesAsync();
        }

        [Test]
        public async Task ShouldCreateUserIfDoesntExist()
        {
            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
            Assert.AreEqual(300, gameUser.Gold);
            Assert.AreEqual("a", gameUser.Character.Name);
            Assert.AreEqual(0, gameUser.Character.Generation);
            Assert.AreEqual(1, gameUser.Character.Level);
            Assert.AreEqual(0, gameUser.Character.Experience);
            Assert.AreEqual(CharacterGender.Male, gameUser.Character.Gender);
            Assert.IsNotEmpty(gameUser.Character.BodyProperties);
            Assert.AreEqual(3, gameUser.Character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, gameUser.Character.Statistics.Attributes.Agility);
            Assert.AreEqual(0, gameUser.Character.Statistics.Attributes.Points);
            Assert.AreEqual(0, gameUser.Character.Statistics.Skills.Points);
            Assert.AreEqual(100, gameUser.Character.Statistics.WeaponProficiencies.Points);
            Assert.AreEqual(5, gameUser.Character.EquippedItems.Count);
            Assert.IsTrue(gameUser.Character.AutoRepair);
            Assert.IsNull(gameUser.Ban);

            // Check that user and its owned entities were created
            var dbUser = await AssertDb.Users
                .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
                .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

            Assert.IsNotNull(dbUser);
            Assert.IsNotEmpty(dbUser.Characters);
            Assert.AreEqual(5, dbUser.Characters[0].EquippedItems.Count);
            Assert.NotZero(dbUser.Characters[0].Statistics.Attributes.Agility);
        }

        [Test]
        public async Task ShouldCreateCharacterIfDoesntExist()
        {
            var user = new User { Platform = Platform.Steam, PlatformUserId = "1", Gold = 1000 };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
            Assert.AreEqual(1000, gameUser.Gold);
            Assert.AreEqual("a", gameUser.Character.Name);
            Assert.AreEqual(0, gameUser.Character.Generation);
            Assert.AreEqual(1, gameUser.Character.Level);
            Assert.AreEqual(0, gameUser.Character.Experience);
            Assert.AreEqual(CharacterGender.Male, gameUser.Character.Gender);
            Assert.IsNotEmpty(gameUser.Character.BodyProperties);
            Assert.AreEqual(3, gameUser.Character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, gameUser.Character.Statistics.Attributes.Agility);
            Assert.AreEqual(0, gameUser.Character.Statistics.Attributes.Points);
            Assert.AreEqual(0, gameUser.Character.Statistics.Skills.Points);
            Assert.AreEqual(100, gameUser.Character.Statistics.WeaponProficiencies.Points);
            Assert.AreEqual(5, gameUser.Character.EquippedItems.Count);
            Assert.IsTrue(gameUser.Character.AutoRepair);
            Assert.IsNull(gameUser.Ban);

            // Check that user and its owned entities were created
            var dbUser = await AssertDb.Users
                .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
                .FirstOrDefaultAsync(u => u.Id == gameUser.Id);

            Assert.IsNotNull(dbUser);
            Assert.IsNotEmpty(dbUser.Characters);
            Assert.AreEqual(5, dbUser.Characters[0].EquippedItems.Count);
            Assert.NotZero(dbUser.Characters[0].Statistics.Attributes.Agility);
        }

        [Test]
        public async Task ShouldNotAddUserItemWhenCreatingCharacterIfItemIsAlreadyOwned()
        {
            var user = new User
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                OwnedItems =
                {
                    // Already owned item
                    new UserItem { ItemId = ArrangeDb.Items.First(i => i.MbId == GetGameUserCommand.Handler.DefaultItemSets[1][0].mbId).Id },
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            // Make sure to always give the same item set to the character
            var randomMock = new Mock<IRandom>();
            randomMock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(1);

            var handler = new GetGameUserCommand.Handler(ActDb, Mapper, Mock.Of<IEventService>(),
                new MachineDateTimeOffset(), randomMock.Object, UserService, CharacterService, Logger);

            // Handle shouldn't throw
            await handler.Handle(new GetGameUserCommand
            {
                Platform = user.Platform,
                PlatformUserId = user.PlatformUserId,
                UserName = "a",
            }, CancellationToken.None);

            var userItems = await AssertDb.UserItems.Where(ui => ui.UserId == user.Id).ToArrayAsync();
            Assert.AreEqual(5, userItems.Length);
        }

        [Test]
        public async Task ShouldGetWithPlatformUserAndPlatform()
        {
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
                new MachineDateTimeOffset(), new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
                new MachineDateTimeOffset(), new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
                dateTime.Object, new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
                dateTime.Object, new ThreadSafeRandom(), UserService, CharacterService, Logger);

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
