using System;
using Crpg.Domain.Entities;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Common.Services
{
    internal interface IStrategusMap
    {
        Point GetSpawnPosition(Region region);
    }

    internal class StrategusMap : IStrategusMap
    {
        private readonly double _width;
        private readonly double _height;
        private readonly double _interactionDistance;

        public StrategusMap(Constants constants)
        {
            _width = constants.StrategusMapWidth;
            _height = constants.StrategusMapHeight;
            _interactionDistance = constants.StrategusInteractionDistance;
        }

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
    }
}
