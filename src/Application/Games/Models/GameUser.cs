using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Games.Models;

public record GameUser : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public int Gold { get; init; }
    public CharacterFullViewModel Character { get; init; } = default!;
    public BanViewModel? Ban { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, GameUser>()
            .ForMember(gu => gu.Character, opt => opt.MapFrom(u => u.Characters.FirstOrDefault()))
            .ForMember(gu => gu.Ban, opt => opt.Ignore());
    }
}
