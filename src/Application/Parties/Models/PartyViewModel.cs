using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

public record PartyViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Region Region { get; init; }
    public int Gold { get; init; }
    public int Troops { get; init; }
    public Point Position { get; init; } = default!;
    public PartyStatus Status { get; init; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    public PartyVisibleViewModel? TargetedParty { get; init; }
    public SettlementPublicViewModel? TargetedSettlement { get; init; }
    public ClanPublicViewModel? Clan { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyViewModel>()
            .ForMember(u => u.Id, opt => opt.MapFrom(u => u.Id))
            .ForMember(u => u.Platform, opt => opt.MapFrom(u => u.User!.Platform))
            .ForMember(u => u.PlatformUserId, opt => opt.MapFrom(u => u.User!.PlatformUserId))
            .ForMember(u => u.Name, opt => opt.MapFrom(u => u.User!.Name))
            .ForMember(h => h.Troops, opt => opt.MapFrom(u => (int)u.Troops))
            .ForMember(u => u.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
    }
}
