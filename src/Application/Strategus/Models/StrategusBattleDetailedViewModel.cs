using System;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleDetailedViewModel
    {
        public int Id { get; init; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;
        public StrategusBattlePhase Phase { get; set; }
        public StrategusBattleFighterViewModel Attacker { get; init; } = default!;
        public int AttackerTotalTroops { get; init; }
        public StrategusBattleFighterViewModel? Defender { get; init; }
        public int DefenderTotalTroops { get; init; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
