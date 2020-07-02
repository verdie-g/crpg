using System;
using System.Collections.Generic;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemViewModel : IMapFrom<Item>
    {
        public int Id { get; set; }
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ItemType Type { get; set; }
        public float Weight { get; set; }

        public ItemArmorComponentViewModel? Armor { get; set; }
        public ItemHorseComponentViewModel? Horse { get; set; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; set; } = Array.Empty<ItemWeaponComponentViewModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Item, ItemViewModel>()
                .ForMember(i => i.Weapons, opt => opt.MapFrom(i => i.GetWeapons()));
        }
    }
}