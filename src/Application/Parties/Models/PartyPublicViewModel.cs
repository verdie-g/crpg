using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View of a <see cref="Party"/> when seen outside of the campaign map (e.g. battle info).
/// </summary>
public record PartyPublicViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public UserPublicViewModel User { get; init; } = default!;
    public ClanPublicViewModel? Clan { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyPublicViewModel>()
            .ForMember(u => u.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
    }
}
