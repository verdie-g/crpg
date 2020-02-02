using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Characters.Commands;

namespace Trpg.Application.UTest.Characters
{
    public class CreateCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var handler = new CreateCharacterCommand.Handler(_db, _mapper);
            var c = await handler.Handle(new CreateCharacterCommand
            {
                Name = "my sword",
                UserId = 4,
            }, CancellationToken.None);

            Assert.NotNull(await _db.Characters.FindAsync(c.Id));
        }

        [Test]
        public void InvalidCharacter()
        {
            var validator = new CreateCharacterCommand.Validator(_db);
            var res = validator.Validate(new CreateCharacterCommand
            {
                Name = "",
                UserId = 4,
            });

            Assert.AreEqual(2, res.Errors.Count);
        }
    }
}