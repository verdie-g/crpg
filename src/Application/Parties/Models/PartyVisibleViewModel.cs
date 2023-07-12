using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Parties;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View of a <see cref="Party"/> when they are visible.
/// </summary>
public record PartyVisibleViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public int Troops { get; init; }
    public Point Position { get; init; } = default!;
    public UserPublicViewModel User { get; init; } = default!;
    public ClanPublicViewModel? Clan { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyVisibleViewModel>()
            .ForMember(h => h.Id, opt => opt.MapFrom(u => u.Id))
            .ForMember(h => h.Troops, opt => opt.MapFrom(u => (int)u.Troops))
            .ForMember(h => h.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
    }
}
