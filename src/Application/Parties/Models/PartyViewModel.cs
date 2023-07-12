using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Parties;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

public record PartyViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public int Gold { get; init; }
    public int Troops { get; init; }
    public Point Position { get; init; } = default!;
    public PartyStatus Status { get; init; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    public PartyVisibleViewModel? TargetedParty { get; init; }
    public SettlementPublicViewModel? TargetedSettlement { get; init; }
    public UserViewModel User { get; init; } = default!;
    public ClanPublicViewModel? Clan { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyViewModel>()
            .ForMember(h => h.Troops, opt => opt.MapFrom(u => (int)u.Troops))
            .ForMember(u => u.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
    }
}
