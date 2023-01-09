using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Api;

internal class StubCrpgClient : ICrpgClient
{
    private static readonly Random Random = new();

    public Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId, CancellationToken cancellationToken = default)
    {
        CrpgUser user = new()
        {
            Id = Random.Next(),
            Platform = Platform.Steam,
            PlatformUserId = Guid.NewGuid().ToString(),
            Gold = 0,
            Role = CrpgUserRole.User,
            Character = new CrpgCharacter
            {
                Id = Random.Next(),
                Name = "Peasant",
                Generation = 0,
                Level = 30,
                Experience = 0,
                ExperienceMultiplier = 1.0f,
                ForTournament = false,
                Characteristics = new CrpgCharacterCharacteristics
                {
                    Attributes = new CrpgCharacterAttributes
                    {
                        Points = 0,
                        Strength = 18,
                        Agility = 18,
                    },
                    Skills = new CrpgCharacterSkills
                    {
                        Points = 0,
                        IronFlesh = 5,
                        PowerStrike = 5,
                        PowerDraw = 5,
                        PowerThrow = 5,
                        Athletics = 5,
                        Riding = 5,
                        WeaponMaster = 5,
                        MountedArchery = 5,
                        Shield = 5,
                    },
                    WeaponProficiencies = new CrpgCharacterWeaponProficiencies
                    {
                        Points = 0,
                        OneHanded = 150,
                        TwoHanded = 150,
                        Polearm = 150,
                        Bow = 150,
                        Throwing = 150,
                        Crossbow = 150,
                    },
                },
                EquippedItems = new[]
                {
                    GetRandomEquippedItem(CrpgItemSlot.Head, ItemObject.ItemTypeEnum.HeadArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Body, ItemObject.ItemTypeEnum.BodyArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Leg, ItemObject.ItemTypeEnum.LegArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon0, ItemObject.ItemTypeEnum.TwoHandedWeapon),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon1, ItemObject.ItemTypeEnum.Crossbow),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon2, ItemObject.ItemTypeEnum.Bolts),
                },
                Rating = new CrpgCharacterRating
                {
                    Volatility = 0,
                    Deviation = 0,
                    Value = 0,
                },
            },
            Restrictions = Array.Empty<CrpgRestriction>(),
            ClanMembership = null,
        };

        return Task.FromResult(new CrpgResult<CrpgUser> { Data = user });
    }

    public Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId, CancellationToken cancellationToken = default)
    {
        return GetUserAsync(platform, platformUserId, cancellationToken);
    }

    public Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CrpgResult<CrpgUsersUpdateResponse>
        {
            Data = new CrpgUsersUpdateResponse { UpdateResults = Array.Empty<UpdateCrpgUserResult>() },
        });
    }

    public Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    private CrpgEquippedItem GetRandomEquippedItem(CrpgItemSlot slot, ItemObject.ItemTypeEnum itemType)
    {
        return new CrpgEquippedItem
        {
            Slot = slot,
            UserItem = new CrpgUserItem
            {
                Id = Random.Next(),
                BaseItemId = GetRandomItemId(itemType),
                Rank = 0,
            },
        };
    }

    private string GetRandomItemId(ItemObject.ItemTypeEnum itemType)
    {
        return MBObjectManager.Instance
            .GetObjectTypeList<ItemObject>()
            .GetRandomElementWithPredicate(i => i.StringId.StartsWith("mp_", StringComparison.Ordinal) && i.Type == itemType)
            .StringId;
    }
}
