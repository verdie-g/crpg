using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusSettlementViewModel : IMapFrom<StrategusSettlement>
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public StrategusSettlementType Type { get; set; }
        public Point Position { get; set; } = default!;
        public StrategusUserPublicViewModel? Owner { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StrategusSettlement, StrategusSettlementViewModel>()
                .ForMember(s => s.Owner, opt => opt.MapFrom(s => s.Owner));
        }
    }
}
