using Trpg.Application.Common.Mappings;
using Trpg.Application.Equipments;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters
{
    public class CharacterModelView : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public EquipmentModelView Head { get; set; }
        public EquipmentModelView Body { get; set; }
        public EquipmentModelView Legs { get; set; }
        public EquipmentModelView Gloves { get; set; }
        public EquipmentModelView Weapon { get; set; }
    }
}
