using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
#nullable disable
namespace Crpg.DumpItemsMod
{
    public class CrpgItemValueModel : ItemValueModel
    {
        private float CalculateArmorTier(ArmorComponent armorComponent)
        {
            float num = 1.2f * (float)armorComponent.HeadArmor + 1f * (float)armorComponent.BodyArmor + 1f * (float)armorComponent.LegArmor + 1f * (float)armorComponent.ArmArmor;
            if (armorComponent.Item.ItemType == ItemObject.ItemTypeEnum.LegArmor)
            {
                num *= 1.6f;
            }
            else if (armorComponent.Item.ItemType == ItemObject.ItemTypeEnum.HandArmor)
            {
                num *= 1.7f;
            }
            else if (armorComponent.Item.ItemType == ItemObject.ItemTypeEnum.HeadArmor)
            {
                num *= 1.2f;
            }
            else if (armorComponent.Item.ItemType == ItemObject.ItemTypeEnum.Cape)
            {
                num *= 1.8f;
            }

            return num * 0.1f - 0.4f;
        }

        private float CalculateHorseTier(HorseComponent horseComponent)
        {
            return (horseComponent.IsPackAnimal ? 25f : (0f + 0.6f * (float)horseComponent.Maneuver + (float)horseComponent.Speed + 1.5f * (float)horseComponent.ChargeDamage + 0.1f * (float)horseComponent.HitPoints)) / 13f - 8f;
        }

        private float CalculateSaddleTier(SaddleComponent saddleComponent)
        {
            return 0f;
        }

        private float CalculateWeaponTier(WeaponComponent weaponComponent)
        {
            ItemObject item = weaponComponent.Item;
            WeaponDesign weaponDesign = (item != null) ? item.WeaponDesign : null;
            if (weaponDesign != null)
            {
                float num = this.CalculateTierCraftedWeapon(weaponDesign);
                float num2 = this.CalculateTierMeleeWeapon(weaponComponent);
                return 0.6f * num2 + 0.4f * num;
            }

            return this.CalculateTierNonCraftedWeapon(weaponComponent);
        }

        private float CalculateTierMeleeWeapon(WeaponComponent weaponComponent)
        {
            WeaponComponentData weaponComponentData = weaponComponent.Weapons[0];
            ItemObject item = weaponComponent.Item;
            if (item != null)
            {
                ItemObject.ItemTypeEnum itemType = item.ItemType;
            }

            WeaponClass weaponClass = weaponComponentData.WeaponClass;
            float val = (float)weaponComponentData.ThrustDamage * this.GetFactor(weaponComponentData.ThrustDamageType) * MathF.Pow((float)weaponComponentData.ThrustSpeed * 0.01f, 1.5f);
            float num = (float)weaponComponentData.SwingDamage * this.GetFactor(weaponComponentData.SwingDamageType) * MathF.Pow((float)weaponComponentData.SwingSpeed * 0.01f, 1.5f);
            float num2 = Math.Max(val, num * 1.1f);
            if (weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
            {
                num2 *= 0.8f;
            }

            if (weaponComponentData.WeaponClass == WeaponClass.ThrowingKnife || weaponComponentData.WeaponClass == WeaponClass.ThrowingKnife || weaponComponentData.WeaponClass == WeaponClass.Javelin)
            {
                num2 *= 1.2f;
            }

            float num3 = (float)weaponComponentData.WeaponLength * 0.01f;
            return 0.06f * (num2 * (1f + num3)) - 3.5f;
        }

        private float GetFactor(DamageTypes swingDamageType)
        {
            if (swingDamageType == DamageTypes.Blunt)
            {
                return 1.3f;
            }

            if (swingDamageType != DamageTypes.Pierce)
            {
                return 1f;
            }

            return 1.15f;
        }

        private float CalculateTierNonCraftedWeapon(WeaponComponent weaponComponent)
        {
            ItemObject item = weaponComponent.Item;
            ItemObject.ItemTypeEnum itemTypeEnum = (item != null) ? item.ItemType : ItemObject.ItemTypeEnum.Invalid;
            if (itemTypeEnum == ItemObject.ItemTypeEnum.Crossbow || itemTypeEnum == ItemObject.ItemTypeEnum.Bow || itemTypeEnum == ItemObject.ItemTypeEnum.Musket || itemTypeEnum == ItemObject.ItemTypeEnum.Pistol)
            {
                return this.CalculateRangedWeaponTier(weaponComponent);
            }

            if (itemTypeEnum == ItemObject.ItemTypeEnum.Arrows || itemTypeEnum == ItemObject.ItemTypeEnum.Bolts || itemTypeEnum == ItemObject.ItemTypeEnum.Bullets)
            {
                return this.CalculateAmmoTier(weaponComponent);
            }

            if (itemTypeEnum == ItemObject.ItemTypeEnum.Shield)
            {
                return this.CalculateShieldTier(weaponComponent);
            }

            return 0f;
        }

        private float CalculateRangedWeaponTier(WeaponComponent weaponComponent)
        {
            WeaponComponentData weaponComponentData = weaponComponent.Weapons[0];
            ItemObject item = weaponComponent.Item;
            ItemObject.ItemTypeEnum itemTypeEnum = (item != null) ? item.ItemType : ItemObject.ItemTypeEnum.Invalid;
            float num = (itemTypeEnum == ItemObject.ItemTypeEnum.Musket) ? 0.5f : ((itemTypeEnum == ItemObject.ItemTypeEnum.Crossbow) ? 0.7f : 1f);
            int thrustDamage = weaponComponentData.ThrustDamage;
            int thrustSpeed = weaponComponentData.ThrustSpeed;
            int accuracy = weaponComponentData.Accuracy;
            return num * ((float)thrustDamage * 0.2f + (float)thrustSpeed * 0.02f + (float)accuracy * 0.02f) - 11f;
        }

        private float CalculateShieldTier(WeaponComponent weaponComponent)
        {
            WeaponComponentData weaponComponentData = weaponComponent.Weapons[0];
            return ((float)weaponComponentData.MaxDataValue + 3f * (float)weaponComponentData.BodyArmor + (float)weaponComponentData.ThrustSpeed) / (6f + weaponComponent.Item.Weight) * 0.13f - 3f;
        }

        private float CalculateAmmoTier(WeaponComponent weaponComponent)
        {
            WeaponComponentData weaponComponentData = weaponComponent.Weapons[0];
            float missileDamage = (float)weaponComponentData.MissileDamage;
            int num = Math.Max(0, (int)(weaponComponentData.MaxDataValue - 20));
            return missileDamage + (float)num * 0.1f;
        }

        private float CalculateTierCraftedWeapon(WeaponDesign craftingData)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (WeaponDesignElement weaponDesignElement in craftingData.UsedPieces)
            {
                if (weaponDesignElement.CraftingPiece.IsValid)
                {
                    num += weaponDesignElement.CraftingPiece.PieceTier;
                    num2++;
                    foreach (ValueTuple<CraftingMaterials, int> valueTuple in weaponDesignElement.CraftingPiece.MaterialsUsed)
                    {
                        CraftingMaterials item = valueTuple.Item1;
                        int item2 = valueTuple.Item2;
                        int num5 = (item == CraftingMaterials.Wood) ? -1 : ((item == CraftingMaterials.Iron1) ? 1 : ((item == CraftingMaterials.Iron2) ? 2 : ((item == CraftingMaterials.Iron3) ? 3 : ((item == CraftingMaterials.Iron4) ? 4 : ((item == CraftingMaterials.Iron5) ? 5 : ((item == CraftingMaterials.Iron6) ? 6 : -1))))));
                        if (num5 >= 0)
                        {
                            num3 += item2 * num5;
                            num4 += item2;
                        }
                    }
                }
            }

            if (num4 > 0 && num2 > 0)
            {
                return 0.4f * (1.25f * (float)num / (float)num2) + 0.6f * ((float)num3 * 1.3f / ((float)num4 + 0.6f) - 1.3f);
            }

            if (num2 > 0)
            {
                return (float)num / (float)num2;
            }

            return 0.1f;
        }

        public override int CalculateValue(ItemObject item)
        {
            float num = 1f;
            if (item.ItemComponent != null)
            {
                num = this.GetEquipmentValueFromTier(item.Tierf);
            }

            float num2 = 1f;
            if (item.ItemComponent is ArmorComponent)
            {
                num2 = (float)((item.ItemType == ItemObject.ItemTypeEnum.BodyArmor) ? 120 : ((item.ItemType == ItemObject.ItemTypeEnum.HandArmor) ? 120 : ((item.ItemType == ItemObject.ItemTypeEnum.LegArmor) ? 120 : 100)));
            }
            else if (item.ItemComponent is WeaponComponent)
            {
                num2 = 100f;
            }
            else if (item.ItemComponent is HorseComponent)
            {
                num2 = 100f;
            }
            else if (item.ItemComponent is SaddleComponent)
            {
                num2 = 100f;
            }
            else if (item.ItemComponent is TradeItemComponent)
            {
                num2 = 100f;
            }

            return (int)(num2 * num * (1f + 0.2f * (item.Appearance - 1f)) + 100f * Math.Max(0f, item.Appearance - 1f));
        }

        private float GetWeaponPriceFactor(ItemObject item)
        {
            return 100f;
        }

            public override float GetEquipmentValueFromTier(float itemTierf)
            {
                double a = 0.05;
                double b = 50;
                double c = 100;
                return (float)(a * Math.Pow((double)itemTierf, 2) + b * (double)itemTierf + c);
            }

        public override float CalculateTier(ItemObject item)
        {
            if (item.ItemComponent is ArmorComponent)
            {
                return this.CalculateArmorTier(item.ItemComponent as ArmorComponent);
            }

            if (item.ItemComponent is WeaponComponent)
            {
                return this.CalculateWeaponTier(item.ItemComponent as WeaponComponent);
            }

            if (item.ItemComponent is HorseComponent)
            {
                return this.CalculateHorseTier(item.ItemComponent as HorseComponent);
            }

            if (item.ItemComponent is SaddleComponent)
            {
                return this.CalculateSaddleTier(item.ItemComponent as SaddleComponent);
            }

            return 0f;
        }
    }
}
