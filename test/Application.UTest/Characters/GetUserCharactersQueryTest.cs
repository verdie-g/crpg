using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserCharactersQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User user0 = new();
        User user1 = new();
        ArrangeDb.AddRange(
            new Character
            {
                Name = "toto",
                User = user0,
            },
            new Character
            {
                Name = "titi",
                User = user1,
            },
            new Character
            {
                Name = "tata",
                User = user0,
            });
        await ArrangeDb.SaveChangesAsync();

        GetUserCharactersQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserCharactersQuery { UserId = user0.Id }, CancellationToken.None);

        var characters = result.Data!;
        Assert.That(characters.Count, Is.EqualTo(2));
    }
}
