using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class UpdateCharacterCharacteristicsCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        WeaponProficiencyPointsForAgility = 14,
        WeaponProficiencyPointsForWeaponMasterCoefs = new[] { 10f, 0f }, // wpp = wm * 10
        WeaponProficiencyCostCoefs = new[] { 3f, 0f }, // wpp cost = wp * 3
    };

    [Test]
    public async Task ShouldUpdateIfEnoughPoints()
    {
        Character character = new()
        {
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes
                {
                    Points = 29 + 58,
                    Strength = 1,
                    Agility = 2,
                },
                Skills = new CharacterSkills
                {
                    Points = 45,
                    IronFlesh = 1,
                    PowerStrike = 2,
                    PowerDraw = 3,
                    PowerThrow = 4,
                    Athletics = 5,
                    Riding = 6,
                    WeaponMaster = 7,
                    MountedArchery = 8,
                    Shield = 9,
                },
                WeaponProficiencies = new CharacterWeaponProficiencies
                {
                    OneHanded = 1,
                    TwoHanded = 2,
                    Polearm = 3,
                    Bow = 4,
                    Throwing = 5,
                    Crossbow = 6,
                },
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            UserId = character.UserId,
            CharacterId = character.Id,
            Characteristics = new CharacterCharacteristicsViewModel
            {
                Attributes = new CharacterAttributesViewModel
                {
                    Strength = 30,
                    Agility = 60,
                },
                Skills = new CharacterSkillsViewModel
                {
                    IronFlesh = 10,
                    PowerStrike = 10,
                    PowerDraw = 10,
                    PowerThrow = 10,
                    Athletics = 10,
                    Riding = 10,
                    WeaponMaster = 10,
                    MountedArchery = 10,
                    Shield = 10,
                },
                WeaponProficiencies = new CharacterWeaponProficienciesViewModel
                {
                    OneHanded = 7,
                    TwoHanded = 7,
                    Polearm = 7,
                    Bow = 7,
                    Throwing = 7,
                    Crossbow = 7,
                },
            },
        }, CancellationToken.None);

        var stats = result.Data!;
        Assert.That(stats.Attributes.Points, Is.EqualTo(0));
        Assert.That(stats.Attributes.Strength, Is.EqualTo(30));
        Assert.That(stats.Attributes.Agility, Is.EqualTo(60));
        Assert.That(stats.Skills.Points, Is.EqualTo(0));
        Assert.That(stats.Skills.IronFlesh, Is.EqualTo(10));
        Assert.That(stats.Skills.PowerStrike, Is.EqualTo(10));
        Assert.That(stats.Skills.PowerDraw, Is.EqualTo(10));
        Assert.That(stats.Skills.PowerThrow, Is.EqualTo(10));
        Assert.That(stats.Skills.Athletics, Is.EqualTo(10));
        Assert.That(stats.Skills.Riding, Is.EqualTo(10));
        Assert.That(stats.Skills.WeaponMaster, Is.EqualTo(10));
        Assert.That(stats.Skills.MountedArchery, Is.EqualTo(10));
        Assert.That(stats.Skills.Shield, Is.EqualTo(10));
        Assert.That(stats.WeaponProficiencies.Points, Is.EqualTo(779));
        Assert.That(stats.WeaponProficiencies.OneHanded, Is.EqualTo(7));
        Assert.That(stats.WeaponProficiencies.TwoHanded, Is.EqualTo(7));
        Assert.That(stats.WeaponProficiencies.Polearm, Is.EqualTo(7));
        Assert.That(stats.WeaponProficiencies.Bow, Is.EqualTo(7));
        Assert.That(stats.WeaponProficiencies.Throwing, Is.EqualTo(7));
        Assert.That(stats.WeaponProficiencies.Crossbow, Is.EqualTo(7));
    }

    [Test]
    public async Task IncreaseAgilityShouldIncreaseWeaponProficienciesPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes { Points = 3 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Points = 84 },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Characteristics = new CharacterCharacteristicsViewModel
            {
                Attributes = new CharacterAttributesViewModel { Agility = 1 },
            },
        }, CancellationToken.None);

        var stats = result.Data!;
        Assert.That(stats.Attributes.Agility, Is.EqualTo(1));
        Assert.That(stats.WeaponProficiencies.Points, Is.EqualTo(98));

        result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Characteristics = new CharacterCharacteristicsViewModel
            {
                Attributes = new CharacterAttributesViewModel { Agility = 3 },
                WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 42 },
            },
        }, CancellationToken.None);

        stats = result.Data!;
        Assert.That(stats.Attributes.Agility, Is.EqualTo(3));
        Assert.That(stats.WeaponProficiencies.Points, Is.EqualTo(0));
        Assert.That(stats.WeaponProficiencies.Bow, Is.EqualTo(42));
    }

    [Test]
    public async Task IncreaseWeaponMasterShouldIncreaseWeaponProficienciesPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes { Agility = 9 },
                Skills = new CharacterSkills { Points = 3 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Points = 270 },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Characteristics = new CharacterCharacteristicsViewModel
            {
                Attributes = new CharacterAttributesViewModel { Agility = 9 },
                Skills = new CharacterSkillsViewModel { WeaponMaster = 1 },
            },
        }, CancellationToken.None);

        var stats = result.Data!;
        Assert.That(stats.Skills.WeaponMaster, Is.EqualTo(1));
        Assert.That(stats.WeaponProficiencies.Points, Is.EqualTo(280));

        result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Characteristics = new CharacterCharacteristicsViewModel
            {
                Attributes = new CharacterAttributesViewModel { Agility = 9 },
                Skills = new CharacterSkillsViewModel { WeaponMaster = 3 },
                WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 100 },
            },
        }, CancellationToken.None);

        stats = result.Data!;
        Assert.That(stats.Skills.WeaponMaster, Is.EqualTo(3));
        Assert.That(stats.WeaponProficiencies.Points, Is.EqualTo(0));
        Assert.That(stats.WeaponProficiencies.Bow, Is.EqualTo(100));
    }

    [Test]
    public async Task ShouldntUpdateIfInconsistentStats()
    {
        Character character = new()
        {
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes { Points = 100 },
                Skills = new CharacterSkills { Points = 100 },
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        CharacterCharacteristicsViewModel[] statsObjects =
        {
            new()
            {
                Attributes = new CharacterAttributesViewModel { Strength = 2 },
                Skills = new CharacterSkillsViewModel { IronFlesh = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Strength = 2 },
                Skills = new CharacterSkillsViewModel { PowerStrike = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Strength = 2 },
                Skills = new CharacterSkillsViewModel { PowerDraw = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Strength = 2 },
                Skills = new CharacterSkillsViewModel { PowerThrow = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Agility = 2 },
                Skills = new CharacterSkillsViewModel { Athletics = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Agility = 2 },
                Skills = new CharacterSkillsViewModel { Riding = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Agility = 2 },
                Skills = new CharacterSkillsViewModel { WeaponMaster = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Agility = 5 },
                Skills = new CharacterSkillsViewModel { MountedArchery = 1 },
            },
            new()
            {
                Attributes = new CharacterAttributesViewModel { Agility = 5 },
                Skills = new CharacterSkillsViewModel { Shield = 1 },
            },
        };

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        foreach (var statObject in statsObjects)
        {
            var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
            {
                UserId = character.UserId,
                CharacterId = character.Id,
                Characteristics = statObject,
            }, CancellationToken.None);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.SkillRequirementNotMet));
        }
    }

    [Test]
    public async Task ShouldntUpdateIfNotEnoughAttributePoints()
    {
        Character character = new();
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        var statsObjects = new[]
        {
            new CharacterCharacteristicsViewModel { Attributes = new CharacterAttributesViewModel { Strength = 1 } },
            new CharacterCharacteristicsViewModel { Attributes = new CharacterAttributesViewModel { Agility = 1 } },
        };

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        foreach (var statObject in statsObjects)
        {
            var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
            {
                UserId = character.UserId,
                CharacterId = character.Id,
                Characteristics = statObject,
            }, CancellationToken.None);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughAttributePoints));
        }
    }

    [Test]
    public async Task ShouldntUpdateIfNotEnoughSkillPoints()
    {
        Character character = new();
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        CharacterCharacteristicsViewModel[] statsObjects =
        {
            new() { Skills = new CharacterSkillsViewModel { IronFlesh = 1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerStrike = 1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerDraw = 1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerThrow = 1 } },
            new() { Skills = new CharacterSkillsViewModel { Athletics = 1 } },
            new() { Skills = new CharacterSkillsViewModel { WeaponMaster = 1 } },
            new() { Skills = new CharacterSkillsViewModel { Riding = 1 } },
            new() { Skills = new CharacterSkillsViewModel { MountedArchery = 1 } },
            new() { Skills = new CharacterSkillsViewModel { Shield = 1 } },
        };

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        foreach (var statObject in statsObjects)
        {
            var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
            {
                UserId = character.UserId,
                CharacterId = character.Id,
                Characteristics = statObject,
            }, CancellationToken.None);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughSkillPoints));
        }
    }

    [Test]
    public async Task ShouldntUpdateIfNotEnoughWeaponProficiencyPoints()
    {
        Character character = new();
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        var statsObjects = new[]
        {
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = 1 } },
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = 1 } },
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = 1 } },
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 1 } },
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = 1 } },
            new CharacterCharacteristicsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = 1 } },
        };

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        foreach (var statObject in statsObjects)
        {
            var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
            {
                UserId = character.UserId,
                CharacterId = character.Id,
                Characteristics = statObject,
            }, CancellationToken.None);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughWeaponProficiencyPoints));
        }
    }

    [Test]
    public async Task StatShouldntBeDecreased()
    {
        Character character = new()
        {
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes { Points = 1000 },
                Skills = new CharacterSkills { Points = 1000 },
                WeaponProficiencies = new CharacterWeaponProficiencies { Points = 1000 },
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        CharacterCharacteristicsViewModel[] statsObjects =
        {
            new() { Attributes = new CharacterAttributesViewModel { Agility = -1 } },
            new() { Attributes = new CharacterAttributesViewModel { Strength = -1 } },
            new() { Skills = new CharacterSkillsViewModel { IronFlesh = -1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerStrike = -1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerDraw = -1 } },
            new() { Skills = new CharacterSkillsViewModel { PowerThrow = -1 } },
            new() { Skills = new CharacterSkillsViewModel { Athletics = -1 } },
            new() { Skills = new CharacterSkillsViewModel { Riding = -1 } },
            new() { Skills = new CharacterSkillsViewModel { WeaponMaster = -1 } },
            new() { Skills = new CharacterSkillsViewModel { MountedArchery = -1 } },
            new() { Skills = new CharacterSkillsViewModel { Shield = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = -1 } },
            new() { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = -1 } },
        };

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        foreach (var statObject in statsObjects)
        {
            var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
            {
                UserId = character.UserId,
                CharacterId = character.Id,
                Characteristics = statObject,
            }, CancellationToken.None);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacteristicDecreased));
        }
    }

    [Test]
    public async Task ShouldThrowNotFoundIfCharacterNotFound()
    {
        var user = ArrangeDb.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            UserId = user.Entity.Id,
            CharacterId = 1,
            Characteristics = new CharacterCharacteristicsViewModel(),
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldThrowNotFoundIfUserNotFound()
    {
        UpdateCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>(), Constants);
        var result = await handler.Handle(new UpdateCharacterCharacteristicsCommand
        {
            UserId = 1,
            CharacterId = 1,
            Characteristics = new CharacterCharacteristicsViewModel(),
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
