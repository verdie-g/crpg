using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Games.Models
{
    public class GameCharacter : IMapFrom<Character>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Level { get; set; }

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
            profile.CreateMap<Character, GameCharacter>()
                .ForMember(gc => gc.CharacterId, opt => opt.MapFrom(c => c.Id))
                .ForMember(gc => gc.NextLevelExperience,
                    opt => opt.MapFrom(c => ExperienceTable.GetExperienceForLevel(c.Level + 1)))
                .ForMember(gc => gc.HeadItemMbId, opt => opt.MapFrom(c => c.HeadItem!.MbId))
                .ForMember(gc => gc.CapeItemMbId, opt => opt.MapFrom(c => c.CapeItem!.MbId))
                .ForMember(gc => gc.BodyItemMbId, opt => opt.MapFrom(c => c.BodyItem!.MbId))
                .ForMember(gc => gc.HandItemMbId, opt => opt.MapFrom(c => c.HandItem!.MbId))
                .ForMember(gc => gc.LegItemMbId, opt => opt.MapFrom(c => c.LegItem!.MbId))
                .ForMember(gc => gc.HorseHarnessItemMbId, opt => opt.MapFrom(c => c.HorseHarnessItem!.MbId))
                .ForMember(gc => gc.HorseItemMbId, opt => opt.MapFrom(c => c.HorseItem!.MbId))
                .ForMember(gc => gc.Weapon1ItemMbId, opt => opt.MapFrom(c => c.Weapon1Item!.MbId))
                .ForMember(gc => gc.Weapon2ItemMbId, opt => opt.MapFrom(c => c.Weapon2Item!.MbId))
                .ForMember(gc => gc.Weapon3ItemMbId, opt => opt.MapFrom(c => c.Weapon3Item!.MbId))
                .ForMember(gc => gc.Weapon4ItemMbId, opt => opt.MapFrom(c => c.Weapon4Item!.MbId));
        }
    }
}