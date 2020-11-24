using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class GetUserQueryTest : TestBase
    {
        [Test]
        public async Task TestWhenUserDoesntExist()
        {
            var handler = new GetUserQuery.Handler(ActDb, Mapper);
            var result = await handler.Handle(new GetUserQuery
            {
                UserId = 1,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
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
