using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class CreateCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            var handler = new CreateCharacterCommand.Handler(_db, _mapper);
            var c = await handler.Handle(new CreateCharacterCommand
            {
                Name = "my sword",
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.NotNull(await _db.Characters.FindAsync(c.Id));
        }

        [Test]
        public void UserNotFound()
        {
            var handler = new CreateCharacterCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new CreateCharacterCommand
            {
                Name = "my sword",
                UserId = 4,
            }, CancellationToken.None));
        }

        [Test]
        public void InvalidCharacter()
        {
            var validator = new CreateCharacterCommand.Validator();
            var res = validator.Validate(new CreateCharacterCommand
            {
                Name = "",
            });

            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}