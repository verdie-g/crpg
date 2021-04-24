using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public record ClanWithMemberCountViewModel : IMapFrom<Clan>
    {
        public ClanViewModel Clan { get; init; } = default!;
        public int MemberCount { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Clan, ClanWithMemberCountViewModel>()
                .ForMember(c => c.Clan, opt => opt.MapFrom(c => c))
                .ForMember(c => c.MemberCount, opt => opt.MapFrom(c => c.Members.Count));
        }
    }
}
