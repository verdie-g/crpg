using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;

public class GetGameUserTournamentCommandTest : TestBase
{
    [Test]
    public async Task UserNotFound()
    {
        var res = await new GetGameUserTournamentCommand.Handler(ActDb, Mapper).Handle(new GetGameUserTournamentCommand
        {
            Platform = Platform.Steam,
            PlatformUserId = "123",
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Theory]
    public async Task CharacterForTournamentNotFound(bool hasCharacter)
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "123",
        };
        if (hasCharacter)
        {
            user.Characters.Add(new Character { ForTournament = false });
        }

        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetGameUserTournamentCommand.Handler(ActDb, Mapper).Handle(new GetGameUserTournamentCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterForTournamentNotFound));
    }

    [Test]
    public async Task Ok()
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "123",
            Characters =
            {
                new Character
                {
                    ForTournament = true,
                    EquippedItems =
                    {
                        new EquippedItem { UserItem = new UserItem { Item = new Item() } },
                    },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetGameUserTournamentCommand.Handler(ActDb, Mapper).Handle(new GetGameUserTournamentCommand
        {
            Platform = user.Platform,
            PlatformUserId = user.PlatformUserId,
        }, CancellationToken.None);

        var gameUser = res.Data!;
        Assert.That(res.Errors, Is.Null);
        Assert.That(gameUser.Id, Is.EqualTo(user.Id));
        Assert.That(gameUser.Platform, Is.EqualTo(user.Platform));
        Assert.That(gameUser.PlatformUserId, Is.EqualTo(user.PlatformUserId));
        Assert.That(gameUser.Character.Id, Is.EqualTo(user.Characters[0].Id));
        Assert.That(gameUser.Character.EquippedItems, Is.Not.Empty);
    }
}
