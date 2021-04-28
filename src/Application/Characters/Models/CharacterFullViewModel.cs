using System;
using System.Collections.Generic;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models
{
    public record CharacterFullViewModel : IMapFrom<Character>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Generation { get; init; }
        public int Level { get; init; }
        public int Experience { get; init; }
        public bool SkippedTheFun { get; init; }
        public bool AutoRepair { get; init; }
        public CharacterStatisticsViewModel Statistics { get; init; } = new();
        public IList<EquippedItemViewModel> EquippedItems { get; init; } = Array.Empty<EquippedItemViewModel>();
    }
}
