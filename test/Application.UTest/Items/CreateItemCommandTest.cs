using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Trpg.Application.Items.Commands;
using Trpg.Application.Items.Models;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Items
{
    public class CreateItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var handler = new CreateItemsCommand.Handler(_db);
            await handler.Handle(new CreateItemsCommand
            {
                Items = new []
                {
                    new ItemCreation
                    {
                        Name = "my sword",
                        Value = 100,
                        Type = ItemType.BodyArmor,
                    },
                    new ItemCreation
                    {
                        Name = "toto",
                        Value = 200,
                        Type = ItemType.Bow,
                    }
                }
            }, CancellationToken.None);

            Assert.AreEqual(2, await _db.Items.CountAsync());
        }
    }
}