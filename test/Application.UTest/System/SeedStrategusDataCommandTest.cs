﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Application.System.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedStrategusDataCommandTest : TestBase
    {
        private static readonly Region[] Regions = Enum.GetValues(typeof(Region)).Cast<Region>().ToArray();
        private static readonly IApplicationEnvironment AppEnv = CreateApplicationEnvironment();

        [Test]
        public async Task ShouldAddSettlementIfDoesntExistsInDb()
        {
            var settlementsSource = new Mock<IStrategusSettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(new[]
                {
                    new StrategusSettlementCreation { Name = "a", Position = new Point(0, 0) },
                    new StrategusSettlementCreation { Name = "b", Position = new Point(0, 0) },
                });

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object,
                Mock.Of<IStrategusMap>(), AppEnv);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(2 * Regions.Length, settlements.Length);

            Assert.NotZero(settlements[0].Id);
            Assert.AreEqual("a", settlements[0].Name);
            Assert.AreEqual(Region.Europe, settlements[0].Region);

            Assert.NotZero(settlements[1].Id);
            Assert.AreEqual("a", settlements[1].Name);
            Assert.AreEqual(Region.NorthAmerica, settlements[1].Region);

            Assert.NotZero(settlements[2].Id);
            Assert.AreEqual("a", settlements[2].Name);
            Assert.AreEqual(Region.Asia, settlements[2].Region);

            Assert.AreEqual("b", settlements[3].Name);
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
                    Region = Region.Europe,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
                new StrategusSettlement
                {
                    Name = "a",
                    Type = StrategusSettlementType.Castle,
                    Culture = Culture.Aserai,
                    Region = Region.NorthAmerica,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
                new StrategusSettlement
                {
                    Name = "a",
                    Type = StrategusSettlementType.Castle,
                    Culture = Culture.Aserai,
                    Region = Region.Asia,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
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
                    },
                });

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Europe))
                .Returns(new Point(3, 4));
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.NorthAmerica))
                .Returns(new Point(4, 5));
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Asia))
                .Returns(new Point(5, 6));

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object,
                strategusMapMock.Object, AppEnv);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(Regions.Length, settlements.Length);

            Assert.AreEqual(Region.Europe, settlements[0].Region);
            Assert.AreEqual(new Point(3, 4), settlements[0].Position);
            Assert.AreEqual(Region.NorthAmerica, settlements[1].Region);
            Assert.AreEqual(new Point(4, 5), settlements[1].Position);
            Assert.AreEqual(Region.Asia, settlements[2].Region);
            Assert.AreEqual(new Point(5, 6), settlements[2].Position);
            for (int i = 0; i < settlements.Length; i += 1)
            {
                Assert.AreEqual(dbSettlements[i].Id, settlements[i].Id);
                Assert.AreEqual("a", settlements[i].Name);
                Assert.AreEqual(StrategusSettlementType.Town, settlements[i].Type);
                Assert.AreEqual(Culture.Battania, settlements[i].Culture);
                Assert.AreEqual("def", settlements[i].Scene);
            }
        }

        [Test]
        public async Task ShouldDeleteSettlementIfDoesntExistInSourceAnymore()
        {
            var dbSettlements = Regions
                .Select(r => new StrategusSettlement { Name = "c", Region = r })
                .ToArray();
            ArrangeDb.StrategusSettlements.AddRange(dbSettlements);
            await ArrangeDb.SaveChangesAsync();

            var settlementsSource = new Mock<IStrategusSettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(Array.Empty<StrategusSettlementCreation>());

            var handler = new SeedStrategusDataCommand.Handler(ActDb, settlementsSource.Object,
                Mock.Of<IStrategusMap>(), AppEnv);
            await handler.Handle(new SeedStrategusDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.StrategusSettlements.ToArrayAsync();
            Assert.AreEqual(0, settlements.Length);
        }

        private static IApplicationEnvironment CreateApplicationEnvironment()
        {
            var appEnvMock = new Mock<IApplicationEnvironment>();
            appEnvMock.Setup(e => e.Environment).Returns(HostingEnvironment.Production);
            return appEnvMock.Object;
        }
    }
}
