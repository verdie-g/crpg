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
    public class GetUserItemsQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = ArrangeDb.Users.Add(new User
            {
                OwnedItems = new List<UserItem>
                {
                    new UserItem { Item = new Item() },
                    new UserItem { Item = new Item() },
                }
            });
            await ArrangeDb.SaveChangesAsync();

            var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
                new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

            Assert.AreEqual(2, result.Data!.Count);
        }
    }
}
