using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class ItemArmorComponentViewModel : IMapFrom<ItemArmorComponent>, ICloneable
    {
        public int HeadArmor { get; set; }
        public int BodyArmor { get; set; }
        public int ArmArmor { get; set; }
        public int LegArmor { get; set; }

        public object Clone() => new ItemArmorComponentViewModel
        {
            HeadArmor = HeadArmor,
            BodyArmor = BodyArmor,
            ArmArmor = ArmArmor,
            LegArmor = LegArmor,
        };
    }
}
