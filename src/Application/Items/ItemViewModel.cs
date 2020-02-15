using Trpg.Application.Common.Mappings;
using Trpg.Domain.Entities;

namespace Trpg.Application.Items
{
    public class ItemViewModel : IMapFrom<Item>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public ItemType Type { get; set; }
    }
}