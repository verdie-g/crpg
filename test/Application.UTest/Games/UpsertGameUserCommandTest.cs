using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Commands;
using Crpg.Common;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class UpsertGameUserCommandTest : TestBase
    {
        private static readonly IEventRaiser EventRaiser = Mock.Of<IEventRaiser>();

        [SetUp]
        public override Task SetUp()
        {
            base.SetUp();

            var allDefaultItemMbIds = UpsertGameUserCommand.Handler.DefaultItemsSets
                .SelectMany(i => new[]
                {
                    i.HeadItemMbId,
                    i.BodyItemMbId,
                    i.LegItemMbId,
                    i.Weapon1ItemMbId,
                    i.Weapon2ItemMbId,
                })
                .Distinct()
                .Select(mbId => new Item { MbId = mbId! });

            Db.Items.AddRange(allDefaultItemMbIds);
            return Db.SaveChangesAsync();
        }

        [Test]
        public async Task BannedUser()
        {
            var user = Db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { new Character() },
                Bans = new List<Ban>
                {
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 2, 1)), Reason = "toto" },
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 3, 1)), Reason = "titi" },
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 1, 2)), Reason = "tata" },
                }
            });
            await Db.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 23, 59, 59)));

            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, dateTime.Object);
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = user.Entity.Characters[0].Name,
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.NotNull(gu.Ban);
            Assert.AreEqual(user.Entity.Bans[2].Until, gu.Ban!.Until);
            Assert.AreEqual(user.Entity.Bans[2].Reason, gu.Ban!.Reason);
        }

        [Test]
        public async Task UnbannedUser()
        {
            var user = Db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { new Character() },
                Bans = new List<Ban>
                {
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 2, 1)), Reason = "toto" },
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 3, 1)), Reason = "titi" },
                    new Ban { Until = new DateTimeOffset(new DateTime(2000, 1, 2)), Reason = "tata" },
                }
            });
            await Db.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 2, 0, 0, 1)));

            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, dateTime.Object);
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = user.Entity.Characters[0].Name,
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.Null(gu.Ban);
        }

        [Test]
        public async Task UserAndCharacterWithEquipmentExist()
        {
            var character = Db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
                Items = new CharacterItems
                {
                    HeadItem = new Item { MbId = "head" },
                    CapeItem = new Item { MbId = "cape" },
                    BodyItem = new Item { MbId = "body" },
                    HandItem = new Item { MbId = "hand" },
                    LegItem = new Item { MbId = "leg" },
                    HorseHarnessItem = new Item { MbId = "horseHarness" },
                    HorseItem = new Item { MbId = "horse" },
                    Weapon1Item = new Item { MbId = "weapon1" },
                    Weapon2Item = new Item { MbId = "weapon2" },
                    Weapon3Item = new Item { MbId = "weapon3" },
                    Weapon4Item = new Item { MbId = "weapon4" },
                }
            });
            var user = Db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await Db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = character.Entity.Name,
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.AreEqual(user.Entity.Id, gu.Id);
            Assert.AreEqual(character.Entity.Name, gu.Character.Name);
            Assert.AreEqual(character.Entity.Experience, gu.Character.Experience);
            Assert.AreEqual(character.Entity.Level, gu.Character.Level);
            Assert.AreEqual(character.Entity.Items.HeadItem!.MbId, gu.Character.Items.HeadItemMbId);
            Assert.AreEqual(character.Entity.Items.CapeItem!.MbId, gu.Character.Items.CapeItemMbId);
            Assert.AreEqual(character.Entity.Items.BodyItem!.MbId, gu.Character.Items.BodyItemMbId);
            Assert.AreEqual(character.Entity.Items.HandItem!.MbId, gu.Character.Items.HandItemMbId);
            Assert.AreEqual(character.Entity.Items.LegItem!.MbId, gu.Character.Items.LegItemMbId);
            Assert.AreEqual(character.Entity.Items.HorseHarnessItem!.MbId, gu.Character.Items.HorseHarnessItemMbId);
            Assert.AreEqual(character.Entity.Items.HorseItem!.MbId, gu.Character.Items.HorseItemMbId);
            Assert.AreEqual(character.Entity.Items.Weapon1Item!.MbId, gu.Character.Items.Weapon1ItemMbId);
            Assert.AreEqual(character.Entity.Items.Weapon2Item!.MbId, gu.Character.Items.Weapon2ItemMbId);
            Assert.AreEqual(character.Entity.Items.Weapon3Item!.MbId, gu.Character.Items.Weapon3ItemMbId);
            Assert.AreEqual(character.Entity.Items.Weapon4Item!.MbId, gu.Character.Items.Weapon4ItemMbId);
            Assert.IsNull(gu.Ban);
        }

        [Test]
        public async Task UserAndCharacterWithNoEquipmentExist()
        {
            var character = Db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
            });
            var user = Db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await Db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = character.Entity.Name,
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.AreEqual(user.Entity.Id, gu.Id);
            Assert.AreEqual(character.Entity.Name, gu.Character.Name);
            Assert.AreEqual(character.Entity.Experience, gu.Character.Experience);
            Assert.AreEqual(character.Entity.Level, gu.Character.Level);
            Assert.IsNull(gu.Ban);
        }

        [Test]
        public async Task ShouldCreateCharacterIfDoesntExist()
        {
            var user = Db.Users.Add(new User { SteamId = 123 });
            await Db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = "toto",
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.AreEqual(user.Entity.Id, gu.Id);
            Assert.AreEqual("toto", gu.Character.Name);
            Assert.AreEqual(0, gu.Character.Experience);
            Assert.AreEqual(1, gu.Character.Level);
            Assert.AreEqual(3, gu.Character.Statistics.Attributes.Strength);
            Assert.AreEqual(3, gu.Character.Statistics.Attributes.Agility);
            Assert.NotNull(gu.Character.Items.HeadItemMbId);
            Assert.NotNull(gu.Character.Items.BodyItemMbId);
            Assert.NotNull(gu.Character.Items.LegItemMbId);
            Assert.NotNull(gu.Character.Items.Weapon1ItemMbId);
            Assert.NotNull(gu.Character.Items.Weapon2ItemMbId);
            Assert.IsNull(gu.Ban);

            Assert.DoesNotThrowAsync(() => Db.Characters.FirstAsync(c => c.UserId == user.Entity.Id && c.Name == "toto"));
        }

        [Test]
        public async Task ShouldCreateUserAndCharacterIfUserDoesntExist()
        {
            var handler = new UpsertGameUserCommand.Handler(Db, Mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = 123,
                CharacterName = "toto",
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.AreEqual("toto", gu.Character.Name);
            Assert.AreEqual(0, gu.Character.Experience);
            Assert.AreEqual(1, gu.Character.Level);
            Assert.NotNull(gu.Character.Items.HeadItemMbId);
            Assert.NotNull(gu.Character.Items.BodyItemMbId);
            Assert.NotNull(gu.Character.Items.LegItemMbId);
            Assert.NotNull(gu.Character.Items.Weapon1ItemMbId);
            Assert.NotNull(gu.Character.Items.Weapon2ItemMbId);
            Assert.IsNull(gu.Ban);

            Assert.DoesNotThrowAsync(() => Db.Users.FirstAsync(u => u.SteamId == 123));
            Assert.DoesNotThrowAsync(() => Db.Characters.FirstAsync(c => c.UserId == gu.Id && c.Name == "toto"));
        }
    }
}