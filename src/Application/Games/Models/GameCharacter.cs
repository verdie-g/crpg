using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Games.Models
{
    public class GameCharacter : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Generation { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public CharacterStatisticsViewModel Statistics { get; set; } = new CharacterStatisticsViewModel();
        public GameCharacterItems Items { get; set; } = new GameCharacterItems();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Character, GameCharacter>()
                .ForMember(gc => gc.NextLevelExperience, opt => opt.MapFrom(c => ExperienceTable.GetExperienceForLevel(c.Level + 1)));
        }
    }
}