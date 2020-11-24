using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class ItemMountComponentViewModel : IMapFrom<ItemMountComponent>, ICloneable
    {
        public int BodyLength { get; set; }
        public int ChargeDamage { get; set; }
        public int Maneuver { get; set; }
        public int Speed { get; set; }
        public int HitPoints { get; set; }

        public object Clone() => new ItemMountComponentViewModel
        {
            BodyLength = BodyLength,
            ChargeDamage = ChargeDamage,
            Maneuver = Maneuver,
            Speed = Speed,
            HitPoints = HitPoints,
        };
    }
}
