using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class UpsertUserCommandTest : TestBase
    {
        private static readonly IEventRaiser EventRaiser = Mock.Of<IEventRaiser>();

        [Test]
        public async Task TestWhenUserDoesntExist()
        {
            var handler = new UpsertUserCommand.Handler(_db, _mapper, EventRaiser);
            var user = await handler.Handle(new UpsertUserCommand
            {
                SteamId = 123,
                UserName = "def",
                Avatar = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            }, CancellationToken.None);

            Assert.AreEqual(Role.User, user.Role);
            Assert.AreEqual(300, user.Gold);
            Assert.NotNull(await _db.Users.FindAsync(user.Id));
        }

        [Test]
        public async Task TestWhenUserAlreadyExist()
        {
            var user = new User
            {
                SteamId = 13948192759205810,
                UserName = "def",
                Gold = 1000,
                Role = Role.Admin,
                AvatarSmall = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var handler = new UpsertUserCommand.Handler(_db, _mapper, EventRaiser);
            var createdUser = await handler.Handle(new UpsertUserCommand
            {
                SteamId = 13948192759205810,
                UserName = "def",
                Avatar = new Uri("http://gh.klm"),
                AvatarMedium = new Uri("http://mn.pqr"),
                AvatarFull = new Uri("http://st.vwx")
            }, CancellationToken.None);

            Assert.AreEqual(user.Id, createdUser.Id);

            var dbUser = await _db.Users.FindAsync(user.Id);
            Assert.AreEqual(dbUser.Id, createdUser.Id);
            Assert.AreEqual(dbUser.SteamId, createdUser.SteamId);
            Assert.AreEqual(dbUser.Gold, createdUser.Gold);
            Assert.AreEqual(dbUser.UserName, createdUser.UserName);
            Assert.AreEqual(dbUser.Role, createdUser.Role);
            Assert.AreEqual(new Uri("http://gh.klm"), createdUser.AvatarSmall);
            Assert.AreEqual(new Uri("http://mn.pqr"), createdUser.AvatarMedium);
            Assert.AreEqual(new Uri("http://st.vwx"), createdUser.AvatarFull);
        }

        [Test]
        public void TestValidationValidCommand()
        {
            var validator = new UpsertUserCommand.Validator();
            var res = validator.Validate(new UpsertUserCommand
            {
                SteamId = 28320184920184918,
                UserName = "toto",
                Avatar = new Uri("http://gh.klm"),
                AvatarMedium = new Uri("http://mn.pqr"),
                AvatarFull = new Uri("http://st.vwx")
            });

            Assert.AreEqual(0, res.Errors.Count);
        }

        [Test]
        public void TestValidationInvalidCommand()
        {
            var validator = new UpsertUserCommand.Validator();
            var res = validator.Validate(new UpsertUserCommand
            {
                SteamId = 123,
                UserName = "",
                Avatar = null,
                AvatarMedium = null,
                AvatarFull = null
            });

            Assert.AreEqual(4, res.Errors.Count);
        }
    }
}