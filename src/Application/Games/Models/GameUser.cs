using System.Linq;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Games.Models
{
    public class GameUser : IMapFrom<User>
    {
        public int Id { get; set; }
        public GameCharacter Character { get; set; } = default!;
        public GameBan? Ban { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, GameUser>()
                .ForMember(gu => gu.Character, opt => opt.MapFrom(u => u.Characters.FirstOrDefault()))
                .ForMember(gu => gu.Ban, opt => opt.Ignore());
        }
    }
}