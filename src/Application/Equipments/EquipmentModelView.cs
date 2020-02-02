using Trpg.Application.Common.Mappings;
using Trpg.Domain.Entities;

namespace Trpg.Application.Equipments
{
    public class EquipmentModelView : IMapFrom<Equipment>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public EquipmentType Type { get; set; }
    }
}