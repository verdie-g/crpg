using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class GetUserQueryTest : TestBase
    {
        [Test]
        public void TestWhenUserDoesntExist()
        {
            var handler = new GetUserQuery.Handler(ActDb, Mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetUserQuery
            {
                UserId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public async Task TestWhenUserExists()
        {
            var dbUser = new User
            {
                PlatformUserId = "13948192759205810",
                Name = "def",
                Role = Role.Admin,
                AvatarSmall = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            };
            ArrangeDb.Users.Add(dbUser);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetUserQuery.Handler(ActDb, Mapper);
            var user = await handler.Handle(new GetUserQuery
            {
                UserId = dbUser.Id,
            }, CancellationToken.None);

            Assert.NotNull(user);
        }
    }
}
