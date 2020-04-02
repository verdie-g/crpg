using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RetireCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var character = _db.Characters.Add(new Character
            {
                Experience = 42424424,
                Level = Constants.MinimumRetiringLevel,
                ExperienceMultiplier = 1.1f,
                User = new User(),
            });
            await _db.SaveChangesAsync();

            await new RetireCharacterCommand.Handler(_db, _mapper).Handle(new RetireCharacterCommand
            {
                CharacterId = character.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(1, character.Entity.Level);
            Assert.AreEqual(0, character.Entity.Experience);
            Assert.AreEqual(1.15f, character.Entity.ExperienceMultiplier);
            Assert.AreEqual(1, character.Entity.User.LoomPoints);
        }

        [Test]
        public void NotFoundIfCharacterDoesntExist()
        {
            Assert.ThrowsAsync<NotFoundException>(() => new RetireCharacterCommand.Handler(_db, _mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = 1,
                }, CancellationToken.None));
        }

        [Test]
        public async Task BadRequestIfLevelTooLow()
        {

            var character = _db.Characters.Add(new Character
            {
                Level = Constants.MinimumRetiringLevel - 1,
                User = new User(),
            });
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<BadRequestException>(() => new RetireCharacterCommand.Handler(_db, _mapper).Handle(
                new RetireCharacterCommand
                {
                    CharacterId = character.Entity.Id,
                }, CancellationToken.None));
        }
    }
}