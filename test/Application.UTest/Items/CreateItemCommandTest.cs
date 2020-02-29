using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class CreateItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var handler = new CreateItemsCommand.Handler(_db);
            await handler.Handle(new CreateItemsCommand
            {
                Items = new[]
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