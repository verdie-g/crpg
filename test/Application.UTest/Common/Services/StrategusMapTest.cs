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
        Assert.That(StrategusMap.ArePointsEquivalent(p1, p2), Is.True);
    }

    [Test]
    public void ArePointsEquivalentShouldReturnFalseIfPointsAreFar()
    {
        Point p1 = new(0, 0);
        Point p2 = new(10, 10);
        Assert.That(StrategusMap.ArePointsEquivalent(p1, p2), Is.False);
    }

    [Test]
    public void ArePointsAtInteractionDistanceShouldReturnTrueIfPointsAreClose()
    {
        Point p1 = new(0.5, 0.5);
        Point p2 = new(1, 0.5);
        Assert.That(StrategusMap.ArePointsAtInteractionDistance(p1, p2), Is.True);
    }

    [Test]
    public void ArePointsAtInteractionDistanceShouldReturnFalseIfPointsAreFar()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        Assert.That(StrategusMap.ArePointsAtInteractionDistance(p1, p2), Is.False);
    }

    [Test]
    public void MovePointTowardsShouldReturnMovedPoint()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        var p3 = StrategusMap.MovePointTowards(p1, p2, 1);
        Assert.That(p3.X, Is.GreaterThan(p1.X));
        Assert.That(p3.Y, Is.GreaterThan(p1.Y));
        Assert.That(p3.X, Is.LessThan(p2.X));
        Assert.That(p3.Y, Is.LessThan(p2.Y));
    }

    [Test]
    public void MovePointTowardsShouldNotMoveFurtherThanTarget()
    {
        Point p1 = new(0, 0);
        Point p2 = new(100, 100);
        var p3 = StrategusMap.MovePointTowards(p1, p2, 10000000);
        Assert.That(p3, Is.EqualTo(p2));
    }

    [TestCase(Region.Eu, Region.Eu, 4, 7)]
    [TestCase(Region.Eu, Region.Na, 1996, 7)]
    [TestCase(Region.Eu, Region.As, 2004, 7)]
    [TestCase(Region.Eu, Region.Oc, 2004, 7)]
    [TestCase(Region.Na, Region.Eu, 1996, 7)]
    [TestCase(Region.Na, Region.Na, 4, 7)]
    [TestCase(Region.Na, Region.As, 3996, 7)]
    [TestCase(Region.Na, Region.Oc, 3996, 7)]
    [TestCase(Region.As, Region.Eu, -1996, 7)]
    [TestCase(Region.As, Region.Na, 3996, 7)]
    [TestCase(Region.As, Region.As, 4, 7)]
    [TestCase(Region.As, Region.Oc, 4, 7)]
    [TestCase(Region.Oc, Region.Eu, -1996, 7)]
    [TestCase(Region.Oc, Region.Na, 3996, 7)]
    [TestCase(Region.Oc, Region.As, 4, 7)]
    [TestCase(Region.Oc, Region.Oc, 4, 7)]
    public void TranslatePositionForRegionTest(Region source, Region target, double expectedX, double expectedY)
    {
        Point p1 = new(4, 7);
        Point p2 = StrategusMap.TranslatePositionForRegion(p1, source, target);
        Assert.That(p2.X, Is.EqualTo(expectedX));
        Assert.That(p2.Y, Is.EqualTo(expectedY));
    }

    [Test]
    public void GetSpawnPositionShouldReturnAPointWithinTheConstantCircle()
    {
        var spawnPosition = StrategusMap.GetSpawnPosition(Region.Eu);
        Assert.That(spawnPosition.X, Is.EqualTo(Constants.StrategusSpawningPositionCenter[0]).Within(Constants.StrategusSpawningPositionRadius));
        Assert.That(spawnPosition.Y, Is.EqualTo(Constants.StrategusSpawningPositionCenter[1]).Within(Constants.StrategusSpawningPositionRadius));
    }
}
