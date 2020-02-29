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
            var handler = new GetUserQuery.Handler(_db, _mapper);
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
                SteamId = 13948192759205810,
                UserName = "def",
                Role = Role.Admin,
                AvatarSmall = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            };
            _db.Users.Add(dbUser);
            await _db.SaveChangesAsync();

            var handler = new GetUserQuery.Handler(_db, _mapper);
            var user = await handler.Handle(new GetUserQuery
            {
                UserId = dbUser.Id,
            }, CancellationToken.None);

            Assert.NotNull(user);
        }
    }
}
