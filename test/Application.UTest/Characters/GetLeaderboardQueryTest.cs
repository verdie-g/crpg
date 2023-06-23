﻿using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetLeaderboardQueryTest : TestBase
{
    [Test]
    public async Task Basic()
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

        User namidaka = new()
        {
            Name = "Namidaka",
            Region = Domain.Entities.Region.Eu,
        };

        User lemon = new()
        {
            Name = "Namidaka",
            Region = Domain.Entities.Region.Na,
        };
        Character orleCharacter = new()
        {
            Name = "shielder",
            UserId = orle.Id,
            User = orle,
            Class = CharacterClass.Infantry,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1800,
            },
        };
        Character takeoCharacter = new()
        {
            Name = "2h",
            UserId = takeo.Id,
            User = takeo,
            Class = CharacterClass.ShockInfantry,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1500,
            },
        };

        Character namidakaCharacter = new()
        {
            Name = "Nami Legodaklas",
            UserId = namidaka.Id,
            User = namidaka,
            Class = CharacterClass.Archer,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1400,
            },
        };

        Character lemonCharacter = new()
        {
            Name = "Salty Lemon",
            UserId = lemon.Id,
            User = lemon,
            Class = CharacterClass.Crossbowman,
            Rating = new()
            {
                Value = 50,
                Deviation = 100,
                Volatility = 100,
                CompetitiveValue = 1000,
            },
        };
        ArrangeDb.Users.Add(orle);
        ArrangeDb.Users.Add(takeo);
        ArrangeDb.Users.Add(namidaka);
        ArrangeDb.Users.Add(lemon);
        ArrangeDb.Characters.Add(takeoCharacter);
        ArrangeDb.Characters.Add(orleCharacter);
        ArrangeDb.Characters.Add(namidakaCharacter);
        ArrangeDb.Characters.Add(lemonCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetLeaderboardQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetLeaderboardQuery
        {
            Region = Domain.Entities.Region.Eu,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.First().Class, Is.EqualTo(CharacterClass.Infantry));
        Assert.That(result.Data!.Last().Class, Is.EqualTo(CharacterClass.Archer));

        GetLeaderboardQuery.Handler handler2 = new(ActDb, Mapper);
        var result2 = await handler.Handle(new GetLeaderboardQuery
        {
            Region = null,
        }, CancellationToken.None);

        Assert.That(result2.Errors, Is.Null);
        Assert.That(result2.Data, Is.Not.Null);
        Assert.That(result2.Data!.First().Class, Is.EqualTo(CharacterClass.Infantry));
        Assert.That(result2.Data!.Last().Class, Is.EqualTo(CharacterClass.Crossbowman));

        GetLeaderboardQuery.Handler handler3 = new(ActDb, Mapper);
        var result3 = await handler.Handle(new GetLeaderboardQuery
        {
            Region = Domain.Entities.Region.Na,
        }, CancellationToken.None);

        Assert.That(result3.Errors, Is.Null);
        Assert.That(result3.Data, Is.Not.Null);
        Assert.That(result3.Data!.First().Class, Is.EqualTo(CharacterClass.Crossbowman));
        Assert.That(result3.Data!.Last().Class, Is.EqualTo(CharacterClass.Crossbowman));
    }
}
