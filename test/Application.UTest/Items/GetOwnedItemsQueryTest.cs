using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class GetOwnedItemsQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                OwnedItems = new List<OwnedItem>
                {
                    new() { Item = new Item() },
                    new() { Item = new Item() },
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var result = await new GetOwnedItemsQuery.Handler(ActDb, Mapper).Handle(
                new GetOwnedItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

            Assert.AreEqual(2, result.Data!.Count);
        }
    }
}
