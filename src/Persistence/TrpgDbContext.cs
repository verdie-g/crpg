using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Trpg.Application.Common.Interfaces;
using Trpg.Common;
using Trpg.Domain.Common;
using Trpg.Domain.Entities;

namespace Trpg.Persistence
{
    public class TrpgDbContext : DbContext, ITrpgDbContext
    {
        private readonly IDateTime _dateTime;

        static TrpgDbContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Role>();
        }

        public TrpgDbContext(DbContextOptions<TrpgDbContext> options)
            : base(options)
        {
        }

        public TrpgDbContext(
            DbContextOptions<TrpgDbContext> options,
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrpgDbContext).Assembly);
            modelBuilder.HasPostgresEnum<Role>();
            modelBuilder.HasPostgresEnum<ItemType>();
        }
    }
}