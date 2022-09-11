using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models;

public record UserPublicViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Uri? Avatar { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserPublicViewModel>()
            .ForMember(u => u.Avatar, opt => opt.MapFrom(u => u.AvatarMedium));
    }
}
