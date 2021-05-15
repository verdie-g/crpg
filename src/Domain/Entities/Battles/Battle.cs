using System.Collections.Generic;
using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Battles
{
    public class Battle : AuditableEntity
    {
        public int Id { get; set; }
        public BattlePhase Phase { get; set; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;

        public List<BattleFighter> Fighters { get; set; } = new();
        public List<FighterApplication> FighterApplications { get; set; } = new();
        public List<BattleMercenary> Mercenaries { get; set; } = new();
        public List<BattleMercenaryApplication> MercenaryApplications { get; set; } = new();
    }
}
