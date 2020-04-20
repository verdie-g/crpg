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
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var handler = new CreateCharacterCommand.Handler(Db, Mapper);
            var c = await handler.Handle(new CreateCharacterCommand
            {
                Name = "my sword",
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.NotNull(await Db.Characters.FindAsync(c.Id));
        }

        [Test]
        public void UserNotFound()
        {
            var handler = new CreateCharacterCommand.Handler(Db, Mapper);
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