using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusUserQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new GetStrategusUserQuery.Handler(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusUserQuery
            {
                UserId = 1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfNotRegisteredToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusUserQuery.Handler(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusUserQuery
            {
                UserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotRegisteredToStrategus, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnStrategusUser()
        {
            var user = new User { StrategusUser = new StrategusUser() };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusUserQuery.Handler(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusUserQuery
            {
                UserId = user.Id,
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
        }
    }
}
