using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleViewModel : IMapFrom<StrategusBattle>
    {
        public int Id { get; init; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;
        public StrategusBattlePhase Phase { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
