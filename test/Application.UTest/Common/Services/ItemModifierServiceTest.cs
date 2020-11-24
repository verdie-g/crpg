using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class ItemModifierServiceTest : TestBase
    {
        [Theory]
        public void StatsShouldIncreaseBetweenRanks(ItemType itemType)
        {
            var item = new ItemCreation
            {
                MbId = "123",
                Name = "toto",
                Value = 1000,
                Type = itemType,
                Weight = 1000f,
                Armor = new ItemArmorComponentViewModel
                {
                    HeadArmor = 100,
                    BodyArmor = 100,
                    ArmArmor = 100,
                    LegArmor = 100,
                },
                Mount = new ItemMountComponentViewModel
                {
                    BodyLength = 100,
                    ChargeDamage = 100,
                    Maneuver = 100,
                    Speed = 100,
                    HitPoints = 100,
                },
                Weapons = new[]
                {
                    new ItemWeaponComponentViewModel
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
                },
            };

            var itemModifierService = new ItemModifierService();
            ItemCreation[] modifiedItems =
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
                StringAssert.EndsWith("123", modifiedItems[i].MbId);
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

                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].Accuracy, modifiedItems[i - 1].Weapons[0].Accuracy);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].MissileSpeed, modifiedItems[i - 1].Weapons[0].MissileSpeed);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].StackAmount, modifiedItems[i - 1].Weapons[0].StackAmount);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].Length, modifiedItems[i - 1].Weapons[0].Length);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].Handling, modifiedItems[i - 1].Weapons[0].Handling);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].BodyArmor, modifiedItems[i - 1].Weapons[0].BodyArmor);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].ThrustDamage, modifiedItems[i - 1].Weapons[0].ThrustDamage);
                Assert.AreEqual(item.Weapons[0].ThrustDamageType, modifiedItems[i].Weapons[0].ThrustDamageType);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].ThrustSpeed, modifiedItems[i - 1].Weapons[0].ThrustSpeed);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].SwingDamage, modifiedItems[i - 1].Weapons[0].SwingDamage);
                Assert.AreEqual(item.Weapons[0].SwingDamageType, modifiedItems[i].Weapons[0].SwingDamageType);
                Assert.GreaterOrEqual(modifiedItems[i].Weapons[0].SwingSpeed, modifiedItems[i - 1].Weapons[0].SwingSpeed);
            }
        }
    }
}
