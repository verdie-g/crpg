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

        DateTimeOffset dt = new(new DateTime(2000, 01, 02));
        Mock<IDateTimeOffset> idt = new();
        idt.SetupGet(i => i.Now).Returns(dt);
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

        DateTimeOffset dt1 = new(new DateTime(2000, 01, 02));
        DateTimeOffset dt2 = new(new DateTime(2000, 01, 03));
        Mock<IDateTimeOffset> idt = new();
        idt.SetupSequence(i => i.Now)
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
