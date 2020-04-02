using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class TickCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var c1 = new Character { Experience = 0, Level = 1, ExperienceMultiplier = 1.05f, User = new User { Gold = 20 } };
            var c2 = new Character { Experience = 300, Level = 1, ExperienceMultiplier = 1f, User = new User { Gold = 30 } };
            _db.AddRange(c1, c2);
            await _db.SaveChangesAsync();

            var cmd = new TickCommand
            {
                Users = new[]
                {
                    new UserTick { CharacterId = c1.Id, ExperienceGain = 200, GoldGain = 50 },
                    new UserTick { CharacterId = c2.Id, ExperienceGain = 300, GoldGain = 100 },
                }
            };

            var res = await new TickCommand.Handler(_db).Handle(cmd, CancellationToken.None);
            Assert.NotNull(res.Users);
            Assert.AreEqual(1, res.Users.Count); // c2 leveled up
            Assert.AreEqual(res.Users[0].UserId, c2.UserId);
            Assert.AreEqual(res.Users[0].Level, 2);
            Assert.AreEqual(res.Users[0].NextLevelExperience, 1360);

            Assert.AreEqual(1, c1.Level);
            Assert.AreEqual(209, c1.Experience);
            Assert.AreEqual(70, c1.User.Gold);

            Assert.AreEqual(2, c2.Level);
            Assert.AreEqual(600, c2.Experience);
            Assert.AreEqual(130, c2.User.Gold);
        }

        [Test]
        public async Task PassTwoLevelInOneTick()
        {
            var c = new Character { Experience = 599, Level = 1, ExperienceMultiplier = 1f, User = new User() };
            _db.Add(c);
            await _db.SaveChangesAsync();

            var cmd = new TickCommand
            {
                Users = new[]
                {
                    new UserTick { CharacterId = c.Id, ExperienceGain = 761, GoldGain = 50 },
                }
            };

            var res = await new TickCommand.Handler(_db).Handle(cmd, CancellationToken.None);
            Assert.NotNull(res.Users);
            Assert.AreEqual(1, res.Users.Count);
            Assert.AreEqual(res.Users[0].UserId, c.UserId);
            Assert.AreEqual(res.Users[0].Level, 3);
            Assert.AreEqual(res.Users[0].NextLevelExperience, 2296);

            Assert.AreEqual(3, c.Level);
            Assert.AreEqual(1360, c.Experience);
        }

        [Test]
        public async Task TickDoesntAffectOtherUserCharacters()
        {
            var user = new User
            {
                Characters = new List<Character>
                {
                    new Character { Experience = 0, Level = 1, ExperienceMultiplier = 1f },
                    new Character { Experience = 0, Level = 1, ExperienceMultiplier = 1f },
                }
            };
            _db.Add(user);
            await _db.SaveChangesAsync();

            var cmd = new TickCommand
            {
                Users = new[]
                {
                    new UserTick { CharacterId = user.Characters[0].Id, ExperienceGain = 200, GoldGain = 50 },
                }
            };

            var res = await new TickCommand.Handler(_db).Handle(cmd, CancellationToken.None);
            Assert.AreEqual(0, res.Users.Count);

            Assert.AreEqual(200, user.Characters[0].Experience);
            Assert.AreEqual(0, user.Characters[1].Experience);
        }
    }
}