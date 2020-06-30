using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemArmorComponentViewModel : IMapFrom<ItemArmorComponentViewModel>, IMapFrom<ItemArmorComponent>
    {
        public int HeadArmor { get; set; }
        public int BodyArmor { get; set; }
        public int ArmArmor { get; set; }
        public int LegArmor { get; set; }
    }
}