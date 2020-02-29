using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Games;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters
{
    public class CharacterViewModel : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Level { get; set; }
        public ItemViewModel HeadItem { get; set; }
        public ItemViewModel CapeItem { get; set; }
        public ItemViewModel BodyItem { get; set; }
        public ItemViewModel HandItem { get; set; }
        public ItemViewModel LegItem { get; set; }
        public ItemViewModel HorseItem { get; set; }
        public ItemViewModel HorseHarnessItem { get; set; }
        public ItemViewModel Weapon1Item { get; set; }
        public ItemViewModel Weapon2Item { get; set; }
        public ItemViewModel Weapon3Item { get; set; }
        public ItemViewModel Weapon4Item { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Character, CharacterViewModel>()
                .ForMember(c => c.NextLevelExperience,
                    opt => opt.MapFrom(c => ExperienceTable.GetExperienceForLevel(c.Level + 1)));
        }
    }
}
