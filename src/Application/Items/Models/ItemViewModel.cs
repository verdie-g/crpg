using Trpg.Application.Common.Mappings;
using Trpg.Domain.Entities;

namespace Trpg.Application.Items.Models
{
    public class ItemViewModel : ItemCreation, IMapFrom<Item>
    {
        public int Id { get; set; }
    }
}