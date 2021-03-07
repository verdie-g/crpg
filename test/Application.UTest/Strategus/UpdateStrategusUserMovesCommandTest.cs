using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Commands;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusUserMovesCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new UpdateStrategusUserMovesCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovesCommand
            {
                UserId = 1,
                Moves = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorUserNotRegisteredToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovesCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovesCommand
            {
                UserId = user.Id,
                Moves = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotRegisteredToStrategus, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldUpdateStrategusUserMoves()
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Moves = new MultiPoint(new[] { new Point(3, 4) })
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovesCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovesCommand
            {
                UserId = user.Id,
                Moves = new MultiPoint(new[]
                {
                    new Point(4, 5),
                    new Point(6, 7),
                }),
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(2, strategusUser.Moves.Count);
            Assert.AreEqual(new Point(4, 5), strategusUser.Moves[0]);
            Assert.AreEqual(new Point(6, 7), strategusUser.Moves[1]);
        }
    }
}
