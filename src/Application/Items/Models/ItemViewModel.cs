using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record ItemViewModel : IMapFrom<Item>
{
    public string Id { get; init; } = string.Empty;
    public string BaseId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Culture Culture { get; init; }
    public ItemType Type { get; init; }
    public int Price { get; init; }
    public float Tier { get; init; }
    public int Requirement { get; init; }
    public float Weight { get; init; }
    public ItemFlags Flags { get; init; }

    public ItemArmorComponentViewModel? Armor { get; init; }
    public ItemMountComponentViewModel? Mount { get; init; }
    public IList<ItemWeaponComponentViewModel> Weapons { get; init; } = Array.Empty<ItemWeaponComponentViewModel>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Item, ItemViewModel>()
            .ForMember(i => i.Weapons, opt => opt.MapFrom(i => i.GetWeapons()));
    }
}
