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
            var character = _db.Characters.Add(new Character { Name = "toto" });
            var user = _db.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity },
            });
            await _db.SaveChangesAsync();

            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Name = "tata",
            };

            var res = await new UpdateCharacterCommand.Handler(_db, _mapper).Handle(cmd, CancellationToken.None);
            Assert.AreEqual(cmd.Name, res.Name);
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
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
            var character = _db.Characters.Add(new Character());
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
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
            var character = _db.Characters.Add(new Character());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = 1,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public void BadName()
        {
            var validator = new UpdateCharacterCommand.Validator();
            var res = validator.Validate(new UpdateCharacterCommand { Name = "" });
            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}