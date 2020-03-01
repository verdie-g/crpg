using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Common;
using Crpg.Domain.Common;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crpg.Persistence
{
    public class CrpgDbContext : DbContext, ICrpgDbContext
    {
        private readonly IDateTime _dateTime;

        static CrpgDbContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Role>();
        }

        public CrpgDbContext(DbContextOptions<CrpgDbContext> options)
            : base(options)
        {
        }

        public CrpgDbContext(
            DbContextOptions<CrpgDbContext> options,
            IDateTime dateTime)
            : base(options)
        {
            _dateTime = dateTime;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<UserItem> UserItems { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.LastModifiedAt = _dateTime.Now;
                    entry.Entity.CreatedAt = _dateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedAt = _dateTime.Now;
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DBConcurrencyException();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrpgDbContext).Assembly);
            modelBuilder.HasPostgresEnum<Role>();
            modelBuilder.HasPostgresEnum<ItemType>();
        }
    }
}