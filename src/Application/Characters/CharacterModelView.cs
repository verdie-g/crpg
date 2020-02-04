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
        public EquipmentModelView HeadEquipment { get; set; }
        public EquipmentModelView BodyEquipment { get; set; }
        public EquipmentModelView LegsEquipment { get; set; }
        public EquipmentModelView GlovesEquipment { get; set; }
        public EquipmentModelView WeaponEquipment { get; set; }
    }
}
