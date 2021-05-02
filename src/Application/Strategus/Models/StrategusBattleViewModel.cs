using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Application.Strategus.Models
{
    public record StrategusBattleViewModel : IMapFrom<StrategusBattle>
    {
        public int Id { get; init; }
        public Region Region { get; set; }
        public StrategusBattlePhase Phase { get; set; }
        public int? AttackedSettlementId { get; set; }
    }
}
