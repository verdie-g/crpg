using Trpg.Application.Common.Mappings;
using Trpg.Application.Items.Models;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters
{
    public class CharacterViewModel : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
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
    }
}
