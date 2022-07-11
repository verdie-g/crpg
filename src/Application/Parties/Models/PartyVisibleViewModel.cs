using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View of a <see cref="Party"/> when they are visible.
/// </summary>
public record PartyVisibleViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Region Region { get; init; }
    public int Troops { get; init; }
    public Point Position { get; init; } = default!;
    public ClanPublicViewModel? Clan { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyVisibleViewModel>()
            .ForMember(h => h.Id, opt => opt.MapFrom(u => u.Id))
            .ForMember(h => h.Platform, opt => opt.MapFrom(u => u.User!.Platform))
            .ForMember(h => h.PlatformUserId, opt => opt.MapFrom(u => u.User!.PlatformUserId))
            .ForMember(h => h.Name, opt => opt.MapFrom(u => u.User!.Name))
            .ForMember(h => h.Troops, opt => opt.MapFrom(u => (int)u.Troops))
            .ForMember(h => h.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
    }
}
