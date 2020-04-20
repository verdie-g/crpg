using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Common;
using Crpg.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans
{
    public class BanCommandTest : TestBase
    {
        [Test]
        public async Task BanExistingUser()
        {
            var user1 = Db.Users.Add(new User());
            var user2 = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var dt = new Mock<IDateTimeOffset>();
            dt.Setup(d => d.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 1)));

            var ban = await new BanCommand.Handler(Db, Mapper, dt.Object).Handle(new BanCommand
            {
                BannedUserId = user1.Entity.Id,
                Duration = TimeSpan.FromDays(1),
                Reason = "toto",
                BannedByUserId = user2.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(user1.Entity.Id, ban.BannedUserId);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 2)), ban.Until);
            Assert.AreEqual("toto", ban.Reason);
            Assert.AreEqual(user2.Entity.Id, ban.BannedByUserId);
        }

        [Test]
        public async Task BanNonExistingUserShouldThrowNotFound()
        {
            var user2 = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var dt = new Mock<IDateTimeOffset>();
            dt.Setup(d => d.Now).Returns(new DateTimeOffset(new DateTime(2000, 1, 1)));

            Assert.ThrowsAsync<NotFoundException>(() => new BanCommand.Handler(Db, Mapper, dt.Object).Handle(new BanCommand
            {
                BannedUserId = 10,
                Duration = TimeSpan.FromDays(1),
                Reason = "toto",
                BannedByUserId = user2.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public void BanningYourselfShouldReturnError()
        {
            var res = new BanCommand.Validator().Validate(new BanCommand
            {
                BannedUserId = 1,
                Duration = TimeSpan.Zero,
                Reason = "aaa",
                BannedByUserId = 1,
            });

            Assert.AreEqual(1, res.Errors.Count);
        }

        [Test]
        public void EmptyBanReasonShouldTrow()
        {
            var res = new BanCommand.Validator().Validate(new BanCommand
            {
                BannedUserId = 1,
                Duration = TimeSpan.Zero,
                Reason = "",
                BannedByUserId = 2,
            });

            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}