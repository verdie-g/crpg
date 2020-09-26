using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using FluentValidation.Results;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class ConvertCharacterStatisticsCommandTest : TestBase
    {
        [Test]
        public async Task ShouldConvertAttributeToSkillsIfEnoughPoints()
        {
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics { Attributes = new CharacterAttributes { Points = 1 } },
            });
            await ArrangeDb.SaveChangesAsync();

            var stats = await new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper).Handle(
                new ConvertCharacterStatisticsCommand
                {
                    CharacterId = character.Entity.Id,
                    UserId = character.Entity.UserId,
                    Conversion = CharacterStatisticConversion.AttributesToSkills,
                }, CancellationToken.None);

            Assert.AreEqual(0, stats.Attributes.Points);
            Assert.AreEqual(2, stats.Skills.Points);
        }

        [Test]
        public async Task ShouldConvertSkillsToAttributeIfEnoughPoints()
        {
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics { Skills = new CharacterSkills { Points = 2 } },
            });
            await ArrangeDb.SaveChangesAsync();

            var stats = await new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper).Handle(
                new ConvertCharacterStatisticsCommand
                {
                    CharacterId = character.Entity.Id,
                    UserId = character.Entity.UserId,
                    Conversion = CharacterStatisticConversion.SkillsToAttributes
                }, CancellationToken.None);

            Assert.AreEqual(1, stats.Attributes.Points);
            Assert.AreEqual(0, stats.Skills.Points);
        }

        [Test]
        public async Task ShouldNotConvertAttributeToSkillsIfNotEnoughPoints()
        {
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics { Attributes = new CharacterAttributes { Points = 0 } },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new ConvertCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Conversion = CharacterStatisticConversion.AttributesToSkills,
            }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldNotConvertSkillsToAttributeIfNotEnoughPoints()
        {
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics { Skills = new CharacterSkills { Points = 1 } },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new ConvertCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Conversion = CharacterStatisticConversion.SkillsToAttributes
            }, CancellationToken.None));
        }

        [Test]
        public void ShouldThrowIfConversionIsNotInEnum()
        {
            var validator = new ConvertCharacterStatisticsCommand.Validator();
            ValidationResult res = validator.Validate(new ConvertCharacterStatisticsCommand
            {
                Conversion = (CharacterStatisticConversion)10,
            });

            Assert.False(res.IsValid);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterNotFound()
        {
             var user = ArrangeDb.Add(new User());
             await ArrangeDb.SaveChangesAsync();

             var handler = new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper);
             Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new ConvertCharacterStatisticsCommand
             {
                 UserId = user.Entity.Id,
                 CharacterId = 1,
             }, CancellationToken.None));
        }

        [Test]
        public void ShouldThrowNotFoundIfUserNotFound()
        {
             var handler = new ConvertCharacterStatisticsCommand.Handler(ActDb, Mapper);
             Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new ConvertCharacterStatisticsCommand
             {
                 UserId = 1,
                 CharacterId = 1,
             }, CancellationToken.None));
        }
    }
}