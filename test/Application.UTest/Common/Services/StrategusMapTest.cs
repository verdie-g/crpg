using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Sdk;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class StrategusMapTest
{
    private static readonly Constants Constants = new()
    {
        StrategusMapWidth = 1000,
        StrategusMapHeight = 1000,
        StrategusEquivalentDistance = 0.5,
        StrategusInteractionDistance = 2,
        StrategusSpawningPositionCenter = new[] { 10.0, 20.0 },
        StrategusSpawningPositionRadius = 5.0,
    };
    private static readonly StrategusMap StrategusMap = new(Constants, new ThreadSafeRandom());

    [Test]
    public void ArePointsEquivalentShouldReturnTrueIfPointsAreClose()
    {
        Point p1 = new(0.00001, 0.00001);
        Point p2 = new(0.00002, 0.00002);
        Assert.True(StrategusMap.ArePointsEquivalent(p1, p2));
    }

    [Test]
    public void ArePointsEquivalentShouldReturnFalseIfPointsAreFar()
    {
        Point p1 = new(0, 0);
        Point p2 = new(10, 10);
        Assert.False(StrategusMap.ArePointsEquivalent(p1, p2));
    }

    [Test]
    public void ArePointsAtInteractionDistanceShouldReturnTrueIfPointsAreClose()
    {
        Point p1 = new(0.5, 0.5);
        Point p2 = new(1, 0.5);
        Assert.True(StrategusMap.ArePointsAtInteractionDistance(p1, p2));
    }

    [Test]
    public void ArePointsAtInteractionDistanceShouldReturnFalseIfPointsAreFar()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        Assert.False(StrategusMap.ArePointsAtInteractionDistance(p1, p2));
    }

    [Test]
    public void MovePointTowardsShouldReturnMovedPoint()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        var p3 = StrategusMap.MovePointTowards(p1, p2, 1);
        Assert.Greater(p3.X, p1.X);
        Assert.Greater(p3.Y, p1.Y);
        Assert.Less(p3.X, p2.X);
        Assert.Less(p3.Y, p2.Y);
    }

    [Test]
    public void MovePointTowardsShouldNotMoveFurtherThanTarget()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        var p3 = StrategusMap.MovePointTowards(p1, p2, 10000000);
        Assert.AreEqual(p2, p3);
    }

    [TestCase(Region.Europe, Region.Europe, 4, 7)]
    [TestCase(Region.Europe, Region.NorthAmerica, 1996, 7)]
    [TestCase(Region.Europe, Region.Asia, 2004, 7)]
    [TestCase(Region.NorthAmerica, Region.Europe, 1996, 7)]
    [TestCase(Region.NorthAmerica, Region.NorthAmerica, 4, 7)]
    [TestCase(Region.NorthAmerica, Region.Asia, 3996, 7)]
    [TestCase(Region.Asia, Region.Europe, -1996, 7)]
    [TestCase(Region.Asia, Region.NorthAmerica, 3996, 7)]
    [TestCase(Region.Asia, Region.Asia, 4, 7)]
    public void TranslatePositionForRegionTest(Region source, Region target, double expectedX, double expectedY)
    {
        Point p1 = new(4, 7);
        Point p2 = StrategusMap.TranslatePositionForRegion(p1, source, target);
        Assert.AreEqual(expectedX, p2.X);
        Assert.AreEqual(expectedY, p2.Y);
    }

    [Test]
    public void GetSpawnPositionShouldReturnAPointWithinTheConstantCircle()
    {
        var spawnPosition = StrategusMap.GetSpawnPosition(Region.Europe);
        Assert.That(spawnPosition.X, Is.EqualTo(Constants.StrategusSpawningPositionCenter[0]).Within(Constants.StrategusSpawningPositionRadius));
        Assert.That(spawnPosition.Y, Is.EqualTo(Constants.StrategusSpawningPositionCenter[1]).Within(Constants.StrategusSpawningPositionRadius));
    }
}
