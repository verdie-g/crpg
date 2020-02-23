using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemViewModel : ItemCreation, IMapFrom<Item>
    {
        public int Id { get; set; }
    }
}