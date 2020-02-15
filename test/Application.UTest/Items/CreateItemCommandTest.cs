using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Items.Commands;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Items
{
    public class CreateItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var handler = new CreateItemCommand.Handler(_db, _mapper);
            var e = await handler.Handle(new CreateItemCommand
            {
                Name = "my sword",
                Price = 100,
                Type = ItemType.Body,
            }, CancellationToken.None);

            Assert.NotNull(await _db.Items.FindAsync(e.Id));
        }

        [Test]
        public async Task DuplicateName()
        {
            _db.Items.Add(new Item
            {
                Name = "toto",
            });
            await _db.SaveChangesAsync();

            var handler = new CreateItemCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new CreateItemCommand
            {
                Name = "toto",
                Price = 100,
                Type = ItemType.Body,
            }, CancellationToken.None));
        }

        [Test]
        public void InvalidItem()
        {
            var validator = new CreateItemCommand.Validator();
            var res = validator.Validate(new CreateItemCommand
            {
                Name = "",
                Price = 0,
                Type = (ItemType) 500,
            });

            Assert.AreEqual(3, res.Errors.Count);
        }
    }
}