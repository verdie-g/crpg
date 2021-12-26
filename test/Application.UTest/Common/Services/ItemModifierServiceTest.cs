using Crpg.Application.Common.Files;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ItemModifierServiceTest : TestBase
{
    [Theory]
    public void StatsShouldIncreaseBetweenRanks(ItemType itemType)
    {
        Item item = new()
        {
            TemplateMbId = "123",
            Name = "toto",
            Value = 1000,
            Type = itemType,
            Weight = 1000f,
            Armor = new ItemArmorComponent
            {
                HeadArmor = 100,
                BodyArmor = 100,
                ArmArmor = 100,
                LegArmor = 100,
            },
            Mount = new ItemMountComponent
            {
                BodyLength = 100,
                ChargeDamage = 100,
                Maneuver = 100,
                Speed = 100,
                HitPoints = 100,
            },
            PrimaryWeapon = new ItemWeaponComponent
            {
                Accuracy = 100,
                MissileSpeed = 100,
                StackAmount = 100,
                Length = 100,
                Handling = 100,
                BodyArmor = 100,
                ThrustDamage = 100,
                ThrustDamageType = DamageType.Cut,
                ThrustSpeed = 100,
                SwingDamage = 100,
                SwingDamageType = DamageType.Pierce,
                SwingSpeed = 100,
            },
        };

        var itemModifiers = new FileItemModifiersSource().LoadItemModifiers();
        ItemModifierService itemModifierService = new(itemModifiers);
        Item[] modifiedItems =
        {
            itemModifierService.ModifyItem(item, -3),
            itemModifierService.ModifyItem(item, -2),
            itemModifierService.ModifyItem(item, -1),
            itemModifierService.ModifyItem(item, 1),
            itemModifierService.ModifyItem(item, 2),
            itemModifierService.ModifyItem(item, 3),
        };

        for (int i = 1; i < modifiedItems.Length; i += 1)
        {
            StringAssert.EndsWith("123", modifiedItems[i].TemplateMbId);
            StringAssert.EndsWith("toto", modifiedItems[i].Name);
            Assert.AreEqual(itemType, modifiedItems[i].Type);

            Assert.GreaterOrEqual(modifiedItems[i].Value, modifiedItems[i - 1].Value);
            Assert.LessOrEqual(modifiedItems[i].Weight, modifiedItems[i - 1].Weight);

            Assert.GreaterOrEqual(modifiedItems[i].Armor!.HeadArmor, modifiedItems[i - 1].Armor!.HeadArmor);
            Assert.GreaterOrEqual(modifiedItems[i].Armor!.BodyArmor, modifiedItems[i - 1].Armor!.BodyArmor);
            Assert.GreaterOrEqual(modifiedItems[i].Armor!.ArmArmor, modifiedItems[i - 1].Armor!.ArmArmor);
            Assert.GreaterOrEqual(modifiedItems[i].Armor!.LegArmor, modifiedItems[i - 1].Armor!.LegArmor);

            Assert.GreaterOrEqual(modifiedItems[i].Mount!.BodyLength, modifiedItems[i - 1].Mount!.BodyLength);
            Assert.GreaterOrEqual(modifiedItems[i].Mount!.ChargeDamage, modifiedItems[i - 1].Mount!.ChargeDamage);
            Assert.GreaterOrEqual(modifiedItems[i].Mount!.Maneuver, modifiedItems[i - 1].Mount!.Maneuver);
            Assert.GreaterOrEqual(modifiedItems[i].Mount!.Speed, modifiedItems[i - 1].Mount!.Speed);
            Assert.GreaterOrEqual(modifiedItems[i].Mount!.HitPoints, modifiedItems[i - 1].Mount!.HitPoints);

            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.Accuracy, modifiedItems[i - 1].PrimaryWeapon!.Accuracy);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.MissileSpeed, modifiedItems[i - 1].PrimaryWeapon!.MissileSpeed);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.StackAmount, modifiedItems[i - 1].PrimaryWeapon!.StackAmount);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.Length, modifiedItems[i - 1].PrimaryWeapon!.Length);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.Handling, modifiedItems[i - 1].PrimaryWeapon!.Handling);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.BodyArmor, modifiedItems[i - 1].PrimaryWeapon!.BodyArmor);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.ThrustDamage, modifiedItems[i - 1].PrimaryWeapon!.ThrustDamage);
            Assert.AreEqual(item.PrimaryWeapon!.ThrustDamageType, modifiedItems[i].PrimaryWeapon!.ThrustDamageType);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.ThrustSpeed, modifiedItems[i - 1].PrimaryWeapon!.ThrustSpeed);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.SwingDamage, modifiedItems[i - 1].PrimaryWeapon!.SwingDamage);
            Assert.AreEqual(item.PrimaryWeapon!.SwingDamageType, modifiedItems[i].PrimaryWeapon!.SwingDamageType);
            Assert.GreaterOrEqual(modifiedItems[i].PrimaryWeapon!.SwingSpeed, modifiedItems[i - 1].PrimaryWeapon!.SwingSpeed);
        }
    }
}
