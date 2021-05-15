using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models
{
    public record BattleViewModel : IMapFrom<Battle>
    {
        public int Id { get; init; }
        public Region Region { get; init; }
        public Point Position { get; init; } = default!;
        public BattlePhase Phase { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
