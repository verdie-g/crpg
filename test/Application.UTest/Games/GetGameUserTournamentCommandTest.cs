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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.CharacterForTournamentNotFound, res.Errors![0].Code);
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
                        new EquippedItem { UserItem = new UserItem { BaseItem = new Item() } },
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
        Assert.IsNull(res.Errors);
        Assert.AreEqual(user.Id, gameUser.Id);
        Assert.AreEqual(user.Platform, gameUser.Platform);
        Assert.AreEqual(user.PlatformUserId, gameUser.PlatformUserId);
        Assert.AreEqual(user.Characters[0].Id, gameUser.Character.Id);
        Assert.IsNotEmpty(gameUser.Character.EquippedItems);
    }
}
