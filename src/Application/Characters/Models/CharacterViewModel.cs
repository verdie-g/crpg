using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public class CharacterViewModel : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Generation { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public bool AutoRepair { get; set; }
        public string BodyProperties { get; set; } = string.Empty;
        public CharacterGender Gender { get; set; }
        public CharacterStatisticsViewModel Statistics { get; set; } = new CharacterStatisticsViewModel();
        public IList<EquippedItemViewModel> EquippedItems { get; set; } = Array.Empty<EquippedItemViewModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Character, CharacterViewModel>()
                .ForMember(c => c.NextLevelExperience, opt => opt.MapFrom(c => ExperienceTable.GetExperienceForLevel(c.Level + 1)));
        }
    }
}
