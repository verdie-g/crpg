using Trpg.Application.Common.Mappings;
using Trpg.Application.Equipments;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters
{
    public class CharacterViewModel : IMapFrom<Character>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public EquipmentViewModel HeadEquipment { get; set; }
        public EquipmentViewModel BodyEquipment { get; set; }
        public EquipmentViewModel LegsEquipment { get; set; }
        public EquipmentViewModel GlovesEquipment { get; set; }
        public EquipmentViewModel Weapon1Equipment { get; set; }
        public EquipmentViewModel Weapon2Equipment { get; set; }
        public EquipmentViewModel Weapon3Equipment { get; set; }
        public EquipmentViewModel Weapon4Equipment { get; set; }
    }
}
