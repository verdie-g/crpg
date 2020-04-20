using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Games.Models
{
    public class GameCharacterItems : IMapFrom<CharacterItems>
    {
        public string? HeadItemMbId { get; set; }
        public string? CapeItemMbId { get; set; }
        public string? BodyItemMbId { get; set; }
        public string? HandItemMbId { get; set; }
        public string? LegItemMbId { get; set; }
        public string? HorseHarnessItemMbId { get; set; }
        public string? HorseItemMbId { get; set; }
        public string? Weapon1ItemMbId { get; set; }
        public string? Weapon2ItemMbId { get; set; }
        public string? Weapon3ItemMbId { get; set; }
        public string? Weapon4ItemMbId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CharacterItems, GameCharacterItems>()
                .ForMember(gci => gci.HeadItemMbId, opt => opt.MapFrom(c => c.HeadItem!.MbId))
                .ForMember(gci => gci.CapeItemMbId, opt => opt.MapFrom(c => c.CapeItem!.MbId))
                .ForMember(gci => gci.BodyItemMbId, opt => opt.MapFrom(c => c.BodyItem!.MbId))
                .ForMember(gci => gci.HandItemMbId, opt => opt.MapFrom(c => c.HandItem!.MbId))
                .ForMember(gci => gci.LegItemMbId, opt => opt.MapFrom(c => c.LegItem!.MbId))
                .ForMember(gci => gci.HorseHarnessItemMbId, opt => opt.MapFrom(c => c.HorseHarnessItem!.MbId))
                .ForMember(gci => gci.HorseItemMbId, opt => opt.MapFrom(c => c.HorseItem!.MbId))
                .ForMember(gci => gci.Weapon1ItemMbId, opt => opt.MapFrom(c => c.Weapon1Item!.MbId))
                .ForMember(gci => gci.Weapon2ItemMbId, opt => opt.MapFrom(c => c.Weapon2Item!.MbId))
                .ForMember(gci => gci.Weapon3ItemMbId, opt => opt.MapFrom(c => c.Weapon3Item!.MbId))
                .ForMember(gci => gci.Weapon4ItemMbId, opt => opt.MapFrom(c => c.Weapon4Item!.MbId));
        }
    }
}