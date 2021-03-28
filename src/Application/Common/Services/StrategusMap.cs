using System;
using Crpg.Domain.Entities;
using Crpg.Sdk.Abstractions;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Common.Services
{
    internal interface IStrategusMap
    {
        double ViewDistance { get; }

        /// <summary>Checks if two points are close enough to be considered equivalent.</summary>
        bool ArePointsEquivalent(Point pointA, Point pointB);

        /// <summary>Checks if two points are close enough to interact with each other.</summary>
        bool ArePointsAtInteractionDistance(Point pointA, Point pointB);

        /// <summary>
        /// Calculate a position between the points specified by <see cref="current"/> and <see cref="target"/>, moving
        /// no farther than the distance specified by <see cref="maxDistanceDelta"/>.
        /// </summary>
        Point MovePointTowards(Point current, Point target, double maxDistanceDelta);

        /// <summary>Translates a point from <see cref="sourceRegion"/> to <see cref="targetRegion"/>.</summary>
        Point TranslatePositionForRegion(Point pos, Region sourceRegion, Region targetRegion);

        /// <summary>Get the spawning position depending on the region.</summary>
        Point GetSpawnPosition(Region region);
    }

    internal class StrategusMap : IStrategusMap
    {
        private readonly IRandom _random;
        private readonly double _width;
        private readonly double _height;
        private readonly double _interactionDistance;
        private readonly double _equivalentDistance;
        private readonly double _viewDistance;
        private readonly Point _spawningPositionCenter;
        private readonly double _spawningPositionRadius;

        public StrategusMap(Constants constants, IRandom random)
        {
            _random = random;
            _width = constants.StrategusMapWidth;
            _height = constants.StrategusMapHeight;
            _interactionDistance = _width * _height / 30_000;
            _equivalentDistance = _width * _height / 300_000;
            _viewDistance = _width * _height / 2000;
            var spawningPosition = constants.StrategusSpawningPositionCenter;
            _spawningPositionCenter = new Point(spawningPosition[0], spawningPosition[1]);
            _spawningPositionRadius = constants.StrategusSpawningPositionRadius;
        }

        public double ViewDistance => _viewDistance;

        /// <inheritdoc />
        public bool ArePointsEquivalent(Point pointA, Point pointB)
        {
            return pointA.EqualsExact(pointB, _equivalentDistance);
        }

        /// <inheritdoc />
        public bool ArePointsAtInteractionDistance(Point pointA, Point pointB)
        {
            return pointA.EqualsExact(pointB, _interactionDistance);
        }

        /// <inheritdoc />
        public Point MovePointTowards(Point current, Point target, double maxDistanceDelta)
        {
            // TODO: does NetTopologySuite provide this functionality?

            double vectorX = target.X - current.X;
            double vectorY = target.Y - current.Y;

            double distanceSquared = vectorX * vectorX + vectorY * vectorY;
            // Return target if current == target or if their distance is less than maxDistanceDelta.
            if (distanceSquared == 0 || (maxDistanceDelta >= 0 && distanceSquared <= maxDistanceDelta * maxDistanceDelta))
            {
                return (Point)target.Copy();
            }

            double distance = Math.Sqrt(distanceSquared);
            return new Point(
                current.X + vectorX / distance * maxDistanceDelta,
                current.Y + vectorY / distance * maxDistanceDelta);
        }

        public Point TranslatePositionForRegion(Point pos, Region sourceRegion, Region targetRegion)
        {
            if (sourceRegion == targetRegion)
            {
                return (Point)pos.Copy();
            }

            // Europe map is duplicated twice for NorthAmerica and Asia and are put together but NorthAmerica is
            // horizontally mirrored. | EU | AN | AS |
            double x = (sourceRegion, targetRegion) switch
            {
                (Region.Europe, Region.NorthAmerica) => 2 * _width - pos.X,
                (Region.NorthAmerica, Region.Europe) => 2 * _width - pos.X,

                (Region.Europe, Region.Asia) => 2 * _width + pos.X,
                (Region.Asia, Region.Europe) => -2 * _width + pos.X,

                (Region.NorthAmerica, Region.Asia) => 4 * _width - pos.X,
                (Region.Asia, Region.NorthAmerica) => 4 * _width - pos.X,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new Point(x, pos.Y);
        }

        /// <inheritdoc />
        public Point GetSpawnPosition(Region region)
        {
            // https://stackoverflow.com/a/50746409/5407910
            double r = _spawningPositionRadius * Math.Sqrt(_random.NextDouble());
            double theta = _random.NextDouble() * 2 * Math.PI;

            var spawningPosition = new Point(
                _spawningPositionCenter.X + r * Math.Cos(theta),
                _spawningPositionCenter.Y + r * Math.Sin(theta));
            return TranslatePositionForRegion(spawningPosition, Region.Europe, region);
        }
    }
}
