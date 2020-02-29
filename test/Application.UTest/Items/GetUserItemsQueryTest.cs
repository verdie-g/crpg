using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class GetUserItemsQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem { Item = new Item() },
                    new UserItem { Item = new Item() },
                }
            });
            await _db.SaveChangesAsync();

            var items = await new GetUserItemsQuery.Handler(_db, _mapper).Handle(
                new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

            Assert.AreEqual(2, items.Count);
        }
    }
}