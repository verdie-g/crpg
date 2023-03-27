using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using FluentValidation.Results;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class ConvertCharacterCharacteristicsCommandTest : TestBase
{
    [Test]
    public async Task ShouldConvertAttributeToSkillsIfEnoughPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics { Attributes = new CharacterAttributes { Points = 1 } },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new ConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper).Handle(
            new ConvertCharacterCharacteristicsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Conversion = CharacterCharacteristicConversion.AttributesToSkills,
            }, CancellationToken.None);

        var stats = result.Data!;
        Assert.That(stats.Attributes.Points, Is.EqualTo(0));
        Assert.That(stats.Skills.Points, Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldConvertSkillsToAttributeIfEnoughPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics { Skills = new CharacterSkills { Points = 2 } },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new ConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper).Handle(
            new ConvertCharacterCharacteristicsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Conversion = CharacterCharacteristicConversion.SkillsToAttributes,
            }, CancellationToken.None);

        var stats = result.Data!;
        Assert.That(stats.Attributes.Points, Is.EqualTo(1));
        Assert.That(stats.Skills.Points, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotConvertAttributeToSkillsIfNotEnoughPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics { Attributes = new CharacterAttributes { Points = 0 } },
        });
        await ArrangeDb.SaveChangesAsync();

        ConvertCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new ConvertCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Conversion = CharacterCharacteristicConversion.AttributesToSkills,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughAttributePoints));
    }

    [Test]
    public async Task ShouldNotConvertSkillsToAttributeIfNotEnoughPoints()
    {
        var character = ArrangeDb.Add(new Character
        {
            Characteristics = new CharacterCharacteristics { Skills = new CharacterSkills { Points = 1 } },
        });
        await ArrangeDb.SaveChangesAsync();

        ConvertCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new ConvertCharacterCharacteristicsCommand
        {
            CharacterId = character.Entity.Id,
            UserId = character.Entity.UserId,
            Conversion = CharacterCharacteristicConversion.SkillsToAttributes,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughSkillPoints));
    }

    [Test]
    public void ShouldThrowIfConversionIsNotInEnum()
    {
        ConvertCharacterCharacteristicsCommand.Validator validator = new();
        ValidationResult res = validator.Validate(new ConvertCharacterCharacteristicsCommand
        {
            Conversion = (CharacterCharacteristicConversion)10,
        });

        Assert.That(res.IsValid, Is.False);
    }

    [Test]
    public async Task ShouldThrowNotFoundIfCharacterNotFound()
    {
        var user = ArrangeDb.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        ConvertCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new ConvertCharacterCharacteristicsCommand
        {
            UserId = user.Entity.Id,
            CharacterId = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldThrowNotFoundIfUserNotFound()
    {
        ConvertCharacterCharacteristicsCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new ConvertCharacterCharacteristicsCommand
        {
            UserId = 1,
            CharacterId = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
