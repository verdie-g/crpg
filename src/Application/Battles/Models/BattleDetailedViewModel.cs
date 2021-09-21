using System;
using System.Collections.Generic;
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
        public List<BattleFighterViewModel> Attackers { get; set; } = new();
        public int AttackerTotalTroops { get; init; }
        public List<BattleFighterViewModel> Defenders { get; set; } = new();
        public int DefenderTotalTroops { get; init; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
