using System;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models
{
    public record BattleDetailedViewModel
    {
        public int Id { get; init; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;
        public BattlePhase Phase { get; set; }
        public BattleFighterViewModel Attacker { get; init; } = default!;
        public int AttackerTotalTroops { get; init; }
        public BattleFighterViewModel? Defender { get; init; }
        public int DefenderTotalTroops { get; init; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
