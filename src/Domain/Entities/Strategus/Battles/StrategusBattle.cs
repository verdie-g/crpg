using System.Collections.Generic;
using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus.Battles
{
    public class StrategusBattle : AuditableEntity
    {
        public int Id { get; set; }
        public StrategusBattlePhase Phase { get; set; }
        public Region Region { get; set; }
        public Point Position { get; set; } = default!;

        public List<StrategusBattleFighter> Fighters { get; set; } = new();
        public List<StrategusBattleFighterApplication> FighterApplications { get; set; } = new();
        public List<StrategusBattleMercenary> Mercenaries { get; set; } = new();
        public List<StrategusBattleMercenaryApplication> MercenaryApplications { get; set; } = new();
    }
}
