using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models;

public record UserPublicViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Uri? Avatar { get; init; }
    public Region? Region { get; init; }
    public ClanPublicViewModel? Clan{ get; init; } = new();
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserPublicViewModel>()
            .ForMember(u => u.Clan, opt => opt.MapFrom(c => c.ClanMembership != null ? c.ClanMembership.Clan! : null));
    }
}
