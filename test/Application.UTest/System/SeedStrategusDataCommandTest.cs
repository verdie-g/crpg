using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Application.Strategus.Models;
using Crpg.Application.System.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedStrategusDataCommandTest : TestBase
    {
        [Test]
        public async Task ShouldAddSettlementIfDoesntExistsInDb()
        {
            var settlementsSource = new Mock<IStrategusSettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(new[]
                {
                    new StrategusSettlementCreation { Name = "a" },
                    new StrategusSettlementCreation { Name = "b" },
                });

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(2, settlements.Length);
            Assert.AreEqual("a", settlements[0].Name);
            Assert.AreEqual("b", settlements[1].Name);
        }

        [Test]
        public async Task ShouldModifySettlementIfAlreadyExistsInDb()
        {
            var dbSettlements = new[]
            {
                new StrategusSettlement
                {
                    Name = "a",
                    Type = StrategusSettlementType.Castle,
                    Culture = Culture.Aserai,
                    Position = new Point(1, 2),
                    Scene = "abc",
                }
            };
            ArrangeDb.StrategusSettlements.AddRange(dbSettlements);
            await ArrangeDb.SaveChangesAsync();

            var settlementsSource = new Mock<IStrategusSettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(new[]
                {
                    new StrategusSettlementCreation
                    {
                        Name = "a",
                        Type = StrategusSettlementType.Town,
                        Culture = Culture.Battania,
                        Position = new Point(3, 4),
                        Scene = "def",
                    }
                });

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(1, settlements.Length);
            Assert.AreEqual(dbSettlements[0].Id, settlements[0].Id);
            Assert.AreEqual("a", settlements[0].Name);
            Assert.AreEqual(StrategusSettlementType.Town, settlements[0].Type);
            Assert.AreEqual(Culture.Battania, settlements[0].Culture);
            Assert.AreEqual(new Point(3, 4), settlements[0].Position);
            Assert.AreEqual("def", settlements[0].Scene);
        }

        [Test]
        public async Task ShouldDeleteSettlementIfDoesntExistInSourceAnymore()
        {
            var dbSettlements = new[] { new StrategusSettlement { Name = "c" } };
            ArrangeDb.StrategusSettlements.AddRange(dbSettlements);
            await ArrangeDb.SaveChangesAsync();

            var settlementsSource = new Mock<IStrategusSettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(Array.Empty<StrategusSettlementCreation>());

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(0, settlements.Length);
        }
    }
}
