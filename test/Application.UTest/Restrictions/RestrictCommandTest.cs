using System.Runtime.Intrinsics.X86;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;
using RTools_NTS.Util;

namespace Crpg.Application.UTest.Restrictions;

public class RestrictCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        DefaultRating = 1500,
        DefaultRatingDeviation = 350,
        DefaultRatingVolatility = 0.06f,
    };
    [Test]
    public async Task RestrictExistingUser()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        var user2 = ArrangeDb.Users.Add(new User { PlatformUserId = "1234", Name = "toto" });
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper, Constants).Handle(new RestrictCommand
        {
            RestrictedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        var restriction = result.Data!;
        Assert.That(restriction.RestrictedUser!.Id, Is.EqualTo(user1.Entity.Id));
        Assert.That(restriction.Duration, Is.EqualTo(TimeSpan.FromDays(1)));
        Assert.That(restriction.Reason, Is.EqualTo("toto"));
        Assert.That(restriction.RestrictedByUser!.Id, Is.EqualTo(user2.Entity.Id));
        Assert.That(restriction.RestrictedByUser.PlatformUserId, Is.EqualTo(user2.Entity.PlatformUserId));
        Assert.That(restriction.RestrictedByUser.Name, Is.EqualTo(user2.Entity.Name));
    }

    [Test]
    public async Task RestrictNonExistingUserShouldThrowNotFound()
    {
        var user2 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper, Constants).Handle(new RestrictCommand
        {
            RestrictedUserId = 10,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task RestrictedByNonExistingUserShouldThrowNotFound()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper, Constants).Handle(new RestrictCommand
        {
            RestrictedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = 10,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public void RestrictingYourselfShouldReturnError()
    {
        var res = new RestrictCommand.Validator().Validate(new RestrictCommand
        {
            RestrictedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = "aaa",
            RestrictedByUserId = 1,
        });

        Assert.That(res.Errors.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ResetRatingRestrictionShouldResetRating()
    {
        User orle = new()
        {
            Name = "Orle",
            Region = Domain.Entities.Region.Eu,
        };

        User takeo = new()
        {
            Name = "Takeo",
            Region = Domain.Entities.Region.Eu,
        };

        var character1 = new Character
        {
            UserId = takeo.Id,
            User = takeo,
            Name = "takeoFirstCharacter",
            Rating = new CharacterRating { CompetitiveValue = 5000, Deviation = 100, Value = 3000, Volatility = 5 },
        };

        var character2 = new Character
        {
            UserId = takeo.Id,
            User = takeo,
            Name = "takeoSecondCharacter",
            Rating = new CharacterRating { CompetitiveValue = 5000, Deviation = 100, Value = 3000, Volatility = 5 },
        };
        ArrangeDb.Users.Add(orle);
        ArrangeDb.Users.Add(takeo);
        ArrangeDb.Characters.Add(character1);
        ArrangeDb.Characters.Add(character2);
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper, Constants).Handle(new RestrictCommand
        {
            RestrictedUserId = takeo.Id,
            Duration = TimeSpan.FromDays(0),
            Reason = "toto",
            RestrictedByUserId = orle.Id,
            Type = RestrictionType.RatingReset,
        }, CancellationToken.None);

        GetUserCharacterRatingQuery.Handler handler = new(ActDb, Mapper);
        var character1Rating = await handler.Handle(new GetUserCharacterRatingQuery
        {
            CharacterId = character1.Id,
            UserId = 2,
        }, CancellationToken.None);
        var character2Rating = await handler.Handle(new GetUserCharacterRatingQuery
        {
            CharacterId = character2.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(character1Rating.Data!.CompetitiveValue, Is.EqualTo(0));
        Assert.That(character2Rating.Data!.CompetitiveValue, Is.EqualTo(0));
    }

    [Test]
    public void EmptyRestrictionReasonReturnError()
    {
        var res = new RestrictCommand.Validator().Validate(new RestrictCommand
        {
            RestrictedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = string.Empty,
            RestrictedByUserId = 2,
        });

        Assert.That(res.Errors.Count, Is.EqualTo(1));
    }
    }
