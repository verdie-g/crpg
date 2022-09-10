using Crpg.Domain.Entities.Characters;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Persistence.UTest;

public class CrpgDbContextTest
{
    [Test]
    public async Task AuditableEntitySetCreatedAtOnCreation()
    {
        var options = new DbContextOptionsBuilder<CrpgDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        DateTime dt = new(2000, 01, 02);
        Mock<IDateTime> idt = new();
        idt.SetupGet(i => i.UtcNow).Returns(dt);
        CrpgDbContext db = new(options, idt.Object);

        Character character = new();
        db.Add(character);
        await db.SaveChangesAsync();

        Assert.AreEqual(dt, character.UpdatedAt);
        Assert.AreEqual(dt, character.CreatedAt);
    }

    [Test]
    public async Task AuditableEntitySetModifiedAtOnUpdate()
    {
        var options = new DbContextOptionsBuilder<CrpgDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        DateTime dt1 = new(2000, 01, 02);
        DateTime dt2 = new(2000, 01, 03);
        Mock<IDateTime> idt = new();
        idt.SetupSequence(i => i.UtcNow)
            .Returns(dt1) // LastModifiedAt
            .Returns(dt1) // CreatedAt
            .Returns(dt2); // LastModifiedAt
        CrpgDbContext db = new(options, idt.Object);

        Character character = new();
        db.Add(character);
        await db.SaveChangesAsync();

        character.Name = "toto";
        await db.SaveChangesAsync();

        Assert.AreEqual(dt2, character.UpdatedAt);
    }
}
