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
            Id = "123",
            Name = "toto",
            Price = 1000,
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
            Assert.That(modifiedItems[i].Id, Does.EndWith("123"));
            Assert.That(modifiedItems[i].Name, Does.EndWith("toto"));
            Assert.That(modifiedItems[i].Type, Is.EqualTo(itemType));

            Assert.That(modifiedItems[i].Price, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Price));
            Assert.That(modifiedItems[i].Weight, Is.LessThanOrEqualTo(modifiedItems[i - 1].Weight));

            Assert.That(modifiedItems[i].Armor!.HeadArmor, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Armor!.HeadArmor));
            Assert.That(modifiedItems[i].Armor!.BodyArmor, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Armor!.BodyArmor));
            Assert.That(modifiedItems[i].Armor!.ArmArmor, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Armor!.ArmArmor));
            Assert.That(modifiedItems[i].Armor!.LegArmor, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Armor!.LegArmor));

            Assert.That(modifiedItems[i].Mount!.BodyLength, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Mount!.BodyLength));
            Assert.That(modifiedItems[i].Mount!.ChargeDamage, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Mount!.ChargeDamage));
            Assert.That(modifiedItems[i].Mount!.Maneuver, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Mount!.Maneuver));
            Assert.That(modifiedItems[i].Mount!.Speed, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Mount!.Speed));
            Assert.That(modifiedItems[i].Mount!.HitPoints, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].Mount!.HitPoints));

            Assert.That(modifiedItems[i].PrimaryWeapon!.Accuracy, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.Accuracy));
            Assert.That(modifiedItems[i].PrimaryWeapon!.MissileSpeed, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.MissileSpeed));
            Assert.That(modifiedItems[i].PrimaryWeapon!.StackAmount, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.StackAmount));    
            Assert.That(modifiedItems[i].PrimaryWeapon!.Length, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.Length));
            Assert.That(modifiedItems[i].PrimaryWeapon!.Handling, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.Handling));
            Assert.That(modifiedItems[i].PrimaryWeapon!.BodyArmor, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.BodyArmor));
            Assert.That(modifiedItems[i].PrimaryWeapon!.ThrustDamage, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.ThrustDamage));
            Assert.That(modifiedItems[i].PrimaryWeapon!.ThrustDamageType, Is.EqualTo(item.PrimaryWeapon!.ThrustDamageType));
            Assert.That(modifiedItems[i].PrimaryWeapon!.ThrustSpeed, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.ThrustSpeed));
            Assert.That(modifiedItems[i].PrimaryWeapon!.SwingDamage, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.SwingDamage));
            Assert.That(modifiedItems[i].PrimaryWeapon!.SwingDamageType, Is.EqualTo(item.PrimaryWeapon!.SwingDamageType));
            Assert.That(modifiedItems[i].PrimaryWeapon!.SwingSpeed, Is.GreaterThanOrEqualTo(modifiedItems[i - 1].PrimaryWeapon!.SwingSpeed));
        }
    }
}
