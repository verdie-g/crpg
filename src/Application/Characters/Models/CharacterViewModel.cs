using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Games;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Models
{
    public class CharacterViewModel : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Level { get; set; }
        public CharacterItemsViewModel Items { get; set; } = new CharacterItemsViewModel();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Character, CharacterViewModel>()
                .ForMember(c => c.NextLevelExperience, opt => opt.MapFrom(c => ExperienceTable.GetExperienceForLevel(c.Level + 1)));
        }
    }
}
