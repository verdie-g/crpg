using System;
using System.Collections.Generic;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public record ItemViewModel : IMapFrom<Item>
    {
        public int Id { get; init; }
        public string TemplateMbId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int Value { get; init; }
        public ItemType Type { get; init; }
        public Culture Culture { get; init; }
        public float Weight { get; init; }
        public int Rank { get; init; }

        public ItemArmorComponentViewModel? Armor { get; init; }
        public ItemMountComponentViewModel? Mount { get; init; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; init; } = Array.Empty<ItemWeaponComponentViewModel>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Item, ItemViewModel>()
                .ForMember(i => i.Weapons, opt => opt.MapFrom(i => i.GetWeapons()));
        }
    }
}
