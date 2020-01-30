using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Trpg.Application.Users.Commands;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Users
{
    public class UpsertUserCommandTest : TestBase
    {
        [Test]
        public async Task TestWhenUserDoesntExist()
        {
            var handler = new UpsertUserCommand.Handler(_db, _mapper);
            var user = await handler.Handle(new UpsertUserCommand
            {
                SteamId = "abc",
                UserName = "def",
                Avatar = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            }, CancellationToken.None);

            var a = await _db.Users.ToArrayAsync();

            Assert.NotNull(await _db.Users.FindAsync(user.Id));
        }

        [Test]
        public async Task TestWhenUserAlreadyExist()
        {
            var user = new User
            {
                SteamId = "abc",
                UserName = "def",
                Role = Role.Admin,
                Avatar = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var handler = new UpsertUserCommand.Handler(_db, _mapper);
            var createdUser = await handler.Handle(new UpsertUserCommand
            {
                SteamId = "abc",
                UserName = "def",
                Avatar = new Uri("http://gh.klm"),
                AvatarMedium = new Uri("http://mn.pqr"),
                AvatarFull = new Uri("http://st.vwx")
            }, CancellationToken.None);

            Assert.AreEqual(user.Id, createdUser.Id);

            var dbUser = await _db.Users.FindAsync(user.Id);
            Assert.AreEqual(dbUser.Id, createdUser.Id);
            Assert.AreEqual(dbUser.SteamId, createdUser.SteamId);
            Assert.AreEqual(dbUser.UserName, createdUser.UserName);
            Assert.AreEqual(dbUser.Role, createdUser.Role);
            Assert.AreEqual(new Uri("http://gh.klm"), createdUser.Avatar);
            Assert.AreEqual(new Uri("http://mn.pqr"), createdUser.AvatarMedium);
            Assert.AreEqual(new Uri("http://st.vwx"), createdUser.AvatarFull);
        }
    }
}