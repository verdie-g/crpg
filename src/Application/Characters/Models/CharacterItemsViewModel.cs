using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Models
{
    public class CharacterItemsViewModel : IMapFrom<CharacterItems>
    {
        public ItemViewModel? HeadItem { get; set; }
        public ItemViewModel? CapeItem { get; set; }
        public ItemViewModel? BodyItem { get; set; }
        public ItemViewModel? HandItem { get; set; }
        public ItemViewModel? LegItem { get; set; }
        public ItemViewModel? HorseItem { get; set; }
        public ItemViewModel? HorseHarnessItem { get; set; }
        public ItemViewModel? Weapon1Item { get; set; }
        public ItemViewModel? Weapon2Item { get; set; }
        public ItemViewModel? Weapon3Item { get; set; }
        public ItemViewModel? Weapon4Item { get; set; }
        public bool AutoRepair { get; set; }
    }
}