using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var character = Db.Characters.Add(new Character { Name = "toto" });
            var user = Db.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity },
            });
            await Db.SaveChangesAsync();

            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Name = "tata",
            };

            var res = await new UpdateCharacterCommand.Handler(Db, Mapper).Handle(cmd, CancellationToken.None);
            Assert.AreEqual(cmd.Name, res.Name);
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task CharacterNotOwned()
        {
            var character = Db.Characters.Add(new Character());
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task UserNotFound()
        {
            var character = Db.Characters.Add(new Character());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = 1,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [TestCase("")]
        [TestCase("a")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void BadName(string name)
        {
            var validator = new UpdateCharacterCommand.Validator();
            var res = validator.Validate(new UpdateCharacterCommand { Name = name });
            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}