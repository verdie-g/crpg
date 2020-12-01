using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Games.Models
{
    public class GameUser : IMapFrom<User>
    {
        public int Id { get; set; }
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = default!;
        public int Gold { get; set; }
        public CharacterViewModel Character { get; set; } = default!;
        public BanViewModel? Ban { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, GameUser>()
                .ForMember(gu => gu.Character, opt => opt.MapFrom(u => u.Characters.FirstOrDefault()))
                .ForMember(gu => gu.Ban, opt => opt.Ignore());
        }
    }
}
