using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Common;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
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
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Culture>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ItemType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ItemSlot>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<DamageType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<WeaponClass>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanMemberRole>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanInvitationType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanInvitationStatus>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<StrategusHeroStatus>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<StrategusSettlementType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<StrategusBattlePhase>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<StrategusBattleSide>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<StrategusBattleFighterApplicationStatus>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Region>();
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
        public DbSet<OwnedItem> OwnedItems { get; set; } = default!;
        public DbSet<EquippedItem> EquippedItems { get; set; } = default!;
        public DbSet<Ban> Bans { get; set; } = default!;
        public DbSet<Clan> Clans { get; set; } = default!;
        public DbSet<ClanMember> ClanMembers { get; set; } = default!;
        public DbSet<ClanInvitation> ClanInvitations { get; set; } = default!;
        public DbSet<StrategusHero> StrategusHeroes { get; set; } = default!;
        public DbSet<StrategusSettlement> StrategusSettlements { get; set; } = default!;
        public DbSet<StrategusOwnedItem> StrategusOwnedItems { get; set; } = default!;
        public DbSet<StrategusBattle> StrategusBattles { get; set; } = default!;
        public DbSet<StrategusBattleFighter> StrategusBattleFighters { get; set; } = default!;
        public DbSet<StrategusBattleFighterApplication> StrategusBattleFighterApplications { get; set; } = default!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // don't override the value if it was already set. Useful for tests
                    if (entry.Entity.UpdatedAt == default)
                    {
                        entry.Entity.UpdatedAt = _dateTime!.Now;
                    }

                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = _dateTime!.Now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // don't override the value if it was already set. Useful for tests
                    if (!entry.Property(e => e.UpdatedAt).IsModified)
                    {
                        entry.Entity.UpdatedAt = _dateTime!.Now;
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
            modelBuilder.HasPostgresEnum<Culture>();
            modelBuilder.HasPostgresEnum<ItemType>();
            modelBuilder.HasPostgresEnum<ItemSlot>();
            modelBuilder.HasPostgresEnum<DamageType>();
            modelBuilder.HasPostgresEnum<WeaponClass>();
            modelBuilder.HasPostgresEnum<ClanMemberRole>();
            modelBuilder.HasPostgresEnum<ClanInvitationType>();
            modelBuilder.HasPostgresEnum<ClanInvitationStatus>();
            modelBuilder.HasPostgresEnum<StrategusHeroStatus>();
            modelBuilder.HasPostgresEnum<StrategusSettlementType>();
            modelBuilder.HasPostgresEnum<StrategusBattlePhase>();
            modelBuilder.HasPostgresEnum<StrategusBattleSide>();
            modelBuilder.HasPostgresEnum<StrategusBattleFighterApplicationStatus>();
            modelBuilder.HasPostgresEnum<Region>();

            // Ensure that the PostGIS extension is installed.
            modelBuilder.HasPostgresExtension("postgis");
        }
    }
}
