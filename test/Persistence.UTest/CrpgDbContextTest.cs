using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Crpg.Common;
using Crpg.Domain.Entities;
using Crpg.Persistence;

namespace Persistence.UTest
{
    public class CrpgDbContextTest
    {
        [Test]
        public async Task AuditableEntitySetCreatedAtOnCreation()
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dt = new DateTime(2000, 01, 02);
            var idt = new Mock<IDateTime>();
            idt.SetupGet(i => i.Now).Returns(dt);
            var db = new CrpgDbContext(options, idt.Object);

            var character = new Character();
            db.Add(character);
            await db.SaveChangesAsync();

            Assert.AreEqual(dt, character.LastModifiedAt);
            Assert.AreEqual(dt, character.CreatedAt);
        }

        [Test]
        public async Task AuditableEntitySetModifiedAtOnUpdate()
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dt1 = new DateTime(2000, 01, 02);
            var dt2 = new DateTime(2000, 01, 03);
            var idt = new Mock<IDateTime>();
            idt.SetupSequence(i => i.Now)
               .Returns(dt1) // LastModifiedAt
               .Returns(dt1) // CreatedAt
               .Returns(dt2); // LastModifiedAt
            var db = new CrpgDbContext(options, idt.Object);

            var character = new Character();
            db.Add(character);
            await db.SaveChangesAsync();

            character.Name = "toto";
            await db.SaveChangesAsync();

            Assert.AreEqual(dt2, character.LastModifiedAt);
        }
    }
}