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
        public Task SetUp()
        {
            var allDefaultItemMbIds = UpsertGameUserCommand.Handler.DefaultCharacterItems
                .SelectMany(i => new[]
                {
                    i.HeadItemMbId,
                    i.BodyItemMbId,
                    i.LegItemMbId,
                    i.Weapon1ItemMbId,
                    i.Weapon2ItemMbId,
                })
                .Distinct()
                .Select(mbId => new Item { MbId = mbId });

            _db.Items.AddRange(allDefaultItemMbIds);
            return _db.SaveChangesAsync();
        }

        [Test]
        public async Task BannedUser()
        {
            var user = _db.Users.Add(new User
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
            await _db.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 1, 23, 59, 59)));

            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, dateTime.Object);
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = user.Entity.Characters[0].Name,
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.NotNull(gu.Ban);
            Assert.AreEqual(user.Entity.Bans[2].Until, gu.Ban.Until);
            Assert.AreEqual(user.Entity.Bans[2].Reason, gu.Ban.Reason);
        }

        [Test]
        public async Task UnbannedUser()
        {
            var user = _db.Users.Add(new User
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
            await _db.SaveChangesAsync();

            var dateTime = new Mock<IDateTimeOffset>();
            dateTime.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 2, 0, 0, 1)));

            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, dateTime.Object);
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
            var character = _db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
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
            });
            var user = _db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await _db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
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
            Assert.AreEqual(character.Entity.HeadItem.MbId, gu.Character.HeadItemMbId);
            Assert.AreEqual(character.Entity.CapeItem.MbId, gu.Character.CapeItemMbId);
            Assert.AreEqual(character.Entity.BodyItem.MbId, gu.Character.BodyItemMbId);
            Assert.AreEqual(character.Entity.HandItem.MbId, gu.Character.HandItemMbId);
            Assert.AreEqual(character.Entity.LegItem.MbId, gu.Character.LegItemMbId);
            Assert.AreEqual(character.Entity.HorseHarnessItem.MbId, gu.Character.HorseHarnessItemMbId);
            Assert.AreEqual(character.Entity.HorseItem.MbId, gu.Character.HorseItemMbId);
            Assert.AreEqual(character.Entity.Weapon1Item.MbId, gu.Character.Weapon1ItemMbId);
            Assert.AreEqual(character.Entity.Weapon2Item.MbId, gu.Character.Weapon2ItemMbId);
            Assert.AreEqual(character.Entity.Weapon3Item.MbId, gu.Character.Weapon3ItemMbId);
            Assert.AreEqual(character.Entity.Weapon4Item.MbId, gu.Character.Weapon4ItemMbId);
            Assert.IsNull(gu.Ban);
        }

        [Test]
        public async Task UserAndCharacterWithNoEquipmentExist()
        {
            var character = _db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
            });
            var user = _db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await _db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
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
            var user = _db.Users.Add(new User { SteamId = 123 });
            await _db.SaveChangesAsync();

            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
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
            Assert.NotNull(gu.Character.HeadItemMbId);
            Assert.NotNull(gu.Character.BodyItemMbId);
            Assert.NotNull(gu.Character.LegItemMbId);
            Assert.NotNull(gu.Character.Weapon1ItemMbId);
            Assert.NotNull(gu.Character.Weapon2ItemMbId);
            Assert.IsNull(gu.Ban);

            Assert.DoesNotThrowAsync(() => _db.Characters.FirstAsync(c => c.UserId == user.Entity.Id && c.Name == "toto"));
        }

        [Test]
        public async Task ShouldCreateUserAndCharacterIfUserDoesntExist()
        {
            var handler = new UpsertGameUserCommand.Handler(_db, _mapper, EventRaiser, Mock.Of<IDateTimeOffset>());
            var gu = await handler.Handle(new UpsertGameUserCommand
            {
                SteamId = 123,
                CharacterName = "toto",
            }, CancellationToken.None);

            Assert.NotNull(gu);
            Assert.AreEqual("toto", gu.Character.Name);
            Assert.AreEqual(0, gu.Character.Experience);
            Assert.AreEqual(1, gu.Character.Level);
            Assert.NotNull(gu.Character.HeadItemMbId);
            Assert.NotNull(gu.Character.BodyItemMbId);
            Assert.NotNull(gu.Character.LegItemMbId);
            Assert.NotNull(gu.Character.Weapon1ItemMbId);
            Assert.NotNull(gu.Character.Weapon2ItemMbId);
            Assert.IsNull(gu.Ban);

            Assert.DoesNotThrowAsync(() => _db.Users.FirstAsync(u => u.SteamId == 123));
            Assert.DoesNotThrowAsync(() => _db.Characters.FirstAsync(c => c.UserId == gu.Id && c.Name == "toto"));
        }
    }
}