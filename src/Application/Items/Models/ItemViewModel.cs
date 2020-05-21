using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemViewModel : ItemCreation, IMapFrom<Item>
    {
        public int Id { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Item, ItemViewModel>()
                .ForMember(i => i.Weapons, opt => opt.MapFrom(i => i.GetWeapons()));
        }
    }
}