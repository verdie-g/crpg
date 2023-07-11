using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Restrictions.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Games.Models;

public record GameUserViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Gold { get; init; }
    public int HeirloomPoints { get; set; }
    public float ExperienceMultiplier { get; init; }
    public Role Role { get; set; }
    public Region Region { get; set; }
    public DateTime CreatedAt { get; set; }
    public GameCharacterViewModel Character { get; init; } = default!;
    public IList<RestrictionViewModel> Restrictions { get; set; } = Array.Empty<RestrictionViewModel>();
    public GameClanMemberViewModel? ClanMembership { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, GameUserViewModel>()
            .ForMember(gu => gu.Character, opt => opt.MapFrom(u => u.Characters.FirstOrDefault()));
    }
}
