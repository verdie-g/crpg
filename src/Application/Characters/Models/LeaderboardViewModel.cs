using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Rating;
namespace Crpg.Application.Characters.Models;

public record LeaderboardViewModel : IMapFrom<Leaderboard>
{
    public DateTime? LeaderboardLastUpdatedDate { get; set; }
    public List<CharacterViewModel> LeaderboardList { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Leaderboard, LeaderboardViewModel>()
            .ForMember(l => l.LeaderboardLastUpdatedDate, opt => opt.MapFrom(l => l.LeaderboardLastUpdatedDate))
            .ForMember(l => l.LeaderboardList, opt => opt.MapFrom(l => l.LeaderboardList));
    }
}
