using System;
using Crpg.Domain.Entities;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Common.Services
{
    internal interface IStrategusMap
    {
        /// <summary>Get the spawning position depending on the region.</summary>
        Point GetSpawnPosition(Region region);

        /// <summary>Checks if two points are close enough to interact with each other.</summary>
        bool ArePointsAtInteractionDistance(Point pointA, Point pointB);

        /// <summary>Checks if two points are close enough to be considered equivalent.</summary>
        bool ArePointsEquivalent(Point pointA, Point pointB);

        /// <summary>
        /// Calculate a position between the points specified by <see cref="current"/> and <see cref="target"/>, moving
        /// no farther than the distance specified by <see cref="maxDistanceDelta"/>.
        /// </summary>
        Point MovePointTowards(Point current, Point target, double maxDistanceDelta);
    }

    internal class StrategusMap : IStrategusMap
    {
        private readonly double _width;
        private readonly double _height;
        private readonly double _interactionDistance;
        private readonly double _equivalentDistance;

        public StrategusMap(Constants constants)
        {
            _width = constants.StrategusMapWidth;
            _height = constants.StrategusMapHeight;
            _interactionDistance = constants.StrategusInteractionDistance;
            _equivalentDistance = constants.StrategusEquivalentDistance;
        }

        /// <inheritdoc />
        public Point GetSpawnPosition(Region region)
        {
            return region switch
            {
                Region.Europe => new Point(_width / 2.0, _height / 2.0),
                Region.NorthAmerica => new Point(_width + _width / 2.0, _height / 2.0),
                Region.Asia => new Point(2 * _width + _width / 2.0, _height / 2.0),
                _ => throw new ArgumentOutOfRangeException(nameof(region), region, null),
            };
        }

        /// <inheritdoc />
        public bool ArePointsAtInteractionDistance(Point pointA, Point pointB)
        {
            return pointA.EqualsExact(pointB, _interactionDistance);
        }

        /// <inheritdoc />
        public bool ArePointsEquivalent(Point pointA, Point pointB)
        {
            return pointA.EqualsExact(pointB, _equivalentDistance);
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
    }
}
