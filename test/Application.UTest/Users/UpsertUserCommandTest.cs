using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Crpg.Sdk.Abstractions.Events;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class UpsertUserCommandTest : TestBase
    {
        private static readonly IEventService EventService = Mock.Of<IEventService>();

        [Test]
        public async Task TestWhenUserDoesntExist()
        {
            var handler = new UpsertUserCommand.Handler(ActDb, Mapper, EventService);
            var result = await handler.Handle(new UpsertUserCommand
            {
                PlatformUserId = "123",
                Name = "def",
                Avatar = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            }, CancellationToken.None);

            var user = result.Data!;
            Assert.AreEqual(Role.User, user.Role);
            Assert.AreEqual(300, user.Gold);
            Assert.NotNull(await AssertDb.Users.FindAsync(user.Id));
        }

        [Test]
        public async Task TestWhenUserAlreadyExist()
        {
            var user = new User
            {
                PlatformUserId = "13948192759205810",
                Name = "def",
                Gold = 1000,
                Role = Role.Admin,
                AvatarSmall = new Uri("http://ghi.klm"),
                AvatarMedium = new Uri("http://mno.pqr"),
                AvatarFull = new Uri("http://stu.vwx")
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpsertUserCommand.Handler(ActDb, Mapper, EventService);
            var result = await handler.Handle(new UpsertUserCommand
            {
                PlatformUserId = "13948192759205810",
                Name = "def",
                Avatar = new Uri("http://gh.klm"),
                AvatarMedium = new Uri("http://mn.pqr"),
                AvatarFull = new Uri("http://st.vwx")
            }, CancellationToken.None);

            var createdUser = result.Data!;
            Assert.AreEqual(user.Id, createdUser.Id);

            var dbUser = await AssertDb.Users.FindAsync(user.Id);
            Assert.AreEqual(dbUser.Id, createdUser.Id);
            Assert.AreEqual(dbUser.PlatformUserId, createdUser.PlatformUserId);
            Assert.AreEqual(dbUser.Gold, createdUser.Gold);
            Assert.AreEqual(dbUser.Name, createdUser.Name);
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
                PlatformUserId = "28320184920184918",
                Name = "toto",
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
                PlatformUserId = "123",
                Name = "",
            });

            Assert.AreEqual(4, res.Errors.Count);
        }
    }
}
