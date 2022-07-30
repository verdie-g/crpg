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
        Assert.AreEqual(0, stats.Attributes.Points);
        Assert.AreEqual(2, stats.Skills.Points);
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
        Assert.AreEqual(1, stats.Attributes.Points);
        Assert.AreEqual(0, stats.Skills.Points);
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

        Assert.AreEqual(ErrorCode.NotEnoughAttributePoints, result.Errors![0].Code);
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
        Assert.AreEqual(ErrorCode.NotEnoughSkillPoints, result.Errors![0].Code);
    }

    [Test]
    public void ShouldThrowIfConversionIsNotInEnum()
    {
        ConvertCharacterCharacteristicsCommand.Validator validator = new();
        ValidationResult res = validator.Validate(new ConvertCharacterCharacteristicsCommand
        {
            Conversion = (CharacterCharacteristicConversion)10,
        });

        Assert.False(res.IsValid);
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

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
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

        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }
}
