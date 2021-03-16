using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class StrategusMapTest
    {
        private static readonly StrategusMap StrategusMap = new StrategusMap(new Constants
        {
            StrategusMapWidth = 1000,
            StrategusMapHeight = 1000,
        });

        [Test]
        public void ArePointsEquivalentShouldReturnTrueIfPointsAreClose()
        {
            var p1 = new Point(0.00001, 0.00001);
            var p2 = new Point(0.00002, 0.00002);
            Assert.True(StrategusMap.ArePointsEquivalent(p1, p2));
        }

        [Test]
        public void ArePointsEquivalentShouldReturnFalseIfPointsAreFar()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(10, 10);
            Assert.False(StrategusMap.ArePointsEquivalent(p1, p2));
        }

        [Test]
        public void ArePointsAtInteractionDistanceShouldReturnTrueIfPointsAreClose()
        {
            var p1 = new Point(0.5, 0.5);
            var p2 = new Point(1, 0.5);
            Assert.True(StrategusMap.ArePointsAtInteractionDistance(p1, p2));
        }

        [Test]
        public void ArePointsAtInteractionDistanceShouldReturnFalseIfPointsAreFar()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(100, 100);
            Assert.False(StrategusMap.ArePointsAtInteractionDistance(p1, p2));
        }

        [Test]
        public void MovePointTowardsShouldReturnMovedPoint()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(100, 100);
            var p3 = StrategusMap.MovePointTowards(p1, p2, 1);
            Assert.Greater(p3.X, p1.X);
            Assert.Greater(p3.Y, p1.Y);
            Assert.Less(p3.X, p2.X);
            Assert.Less(p3.Y, p2.Y);
        }

        [Test]
        public void MovePointTowardsShouldMoveFurtherThanTarget()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(100, 100);
            var p3 = StrategusMap.MovePointTowards(p1, p2, 10000000);
            Assert.AreEqual(p2, p3);
        }
    }
}
