using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Strategus.Models
{
    public class StrategusHeroViewModel : IMapFrom<StrategusHero>
    {
        public int Id { get; set; }
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Region Region { get; set; }
        public int Silver { get; set; }
        public int Troops { get; set; }
        public Point Position { get; set; } = default!;
        public StrategusHeroStatus Status { get; set; }
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;
        public StrategusHeroPublicViewModel? TargetedHero { get; set; }
        public StrategusSettlementViewModel? TargetedSettlement { get; set; }
        public ClanPublicViewModel? Clan { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StrategusHero, StrategusHeroViewModel>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.UserId))
                .ForMember(u => u.Platform, opt => opt.MapFrom(u => u.User!.Platform))
                .ForMember(u => u.PlatformUserId, opt => opt.MapFrom(u => u.User!.PlatformUserId))
                .ForMember(u => u.Name, opt => opt.MapFrom(u => u.User!.Name))
                .ForMember(u => u.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
        }
    }
}
