using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Common;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crpg.Persistence
{
    public class CrpgDbContext : DbContext, ICrpgDbContext
    {
        private readonly IDateTimeOffset? _dateTime;

        static CrpgDbContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Platform>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Role>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ItemType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DamageType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<WeaponClass>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<CharacterGender>();
        }

        public CrpgDbContext(DbContextOptions<CrpgDbContext> options)
            : base(options)
        {
        }

        public CrpgDbContext(
            DbContextOptions<CrpgDbContext> options,
            IDateTimeOffset dateTime)
            : base(options)
        {
            _dateTime = dateTime;
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Character> Characters { get; set; } = default!;
        public DbSet<Item> Items { get; set; } = default!;
        public DbSet<UserItem> UserItems { get; set; } = default!;
        public DbSet<Ban> Bans { get; set; } = default!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // don't override the value if it was already set. Useful for tests
                    if (entry.Entity.LastModifiedAt == default)
                    {
                        entry.Entity.LastModifiedAt = _dateTime!.Now;
                    }

                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = _dateTime!.Now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // don't override the value if it was already set. Useful for tests
                    if (!entry.Property(e => e.LastModifiedAt).IsModified)
                    {
                        entry.Entity.LastModifiedAt = _dateTime!.Now;
                    }
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new ConflictException(e);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrpgDbContext).Assembly);
            modelBuilder.HasPostgresEnum<Platform>();
            modelBuilder.HasPostgresEnum<Role>();
            modelBuilder.HasPostgresEnum<ItemType>();
            modelBuilder.HasPostgresEnum<DamageType>();
            modelBuilder.HasPostgresEnum<WeaponClass>();
            modelBuilder.HasPostgresEnum<CharacterGender>();
        }
    }
}
