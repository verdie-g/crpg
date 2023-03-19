using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Common;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crpg.Persistence;

public class CrpgDbContext : DbContext, ICrpgDbContext
{
    private readonly IDateTime? _dateTime;

    static CrpgDbContext()
    {
#pragma warning disable CS0618 // Since 7.0, should use NpgsqlDataSourceBuilder but it breaks the migrations. I think there is an issue in npgsql.
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Platform>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Role>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<CharacterClass>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<RestrictionType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Culture>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ItemType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ItemSlot>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DamageType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<WeaponClass>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanMemberRole>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanInvitationType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ClanInvitationStatus>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<PartyStatus>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<SettlementType>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<BattlePhase>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<BattleSide>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<BattleFighterApplicationStatus>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<BattleMercenaryApplicationStatus>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Region>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ActivityLogType>();
#pragma warning restore CS0618
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

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Character> Characters { get; set; } = default!;
    public DbSet<Item> Items { get; set; } = default!;
    public DbSet<UserItem> UserItems { get; set; } = default!;
    public DbSet<EquippedItem> EquippedItems { get; set; } = default!;
    public DbSet<CharacterLimitations> CharacterLimitations { get; set; } = default!;
    public DbSet<Restriction> Restrictions { get; set; } = default!;
    public DbSet<Clan> Clans { get; set; } = default!;
    public DbSet<ClanMember> ClanMembers { get; set; } = default!;
    public DbSet<ClanInvitation> ClanInvitations { get; set; } = default!;
    public DbSet<Party> Parties { get; set; } = default!;
    public DbSet<Settlement> Settlements { get; set; } = default!;
    public DbSet<SettlementItem> SettlementItems { get; set; } = default!;
    public DbSet<PartyItem> PartyItems { get; set; } = default!;
    public DbSet<Battle> Battles { get; set; } = default!;
    public DbSet<BattleFighter> BattleFighters { get; set; } = default!;
    public DbSet<BattleFighterApplication> BattleFighterApplications { get; set; } = default!;
    public DbSet<BattleMercenary> BattleMercenaries { get; set; } = default!;
    public DbSet<BattleMercenaryApplication> BattleMercenaryApplications { get; set; } = default!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = default!;
    public DbSet<ActivityLogMetadata> ActivityLogMetadata { get; set; } = default!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // don't override the value if it was already set. Useful for tests
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = _dateTime!.UtcNow;
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = _dateTime!.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // don't override the value if it was already set. Useful for tests
                if (!entry.Property(e => e.UpdatedAt).IsModified)
                {
                    entry.Entity.UpdatedAt = _dateTime!.UtcNow;
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
        modelBuilder.HasPostgresEnum<CharacterClass>();
        modelBuilder.HasPostgresEnum<RestrictionType>();
        modelBuilder.HasPostgresEnum<Culture>();
        modelBuilder.HasPostgresEnum<ItemType>();
        modelBuilder.HasPostgresEnum<ItemSlot>();
        modelBuilder.HasPostgresEnum<DamageType>();
        modelBuilder.HasPostgresEnum<WeaponClass>();
        modelBuilder.HasPostgresEnum<ClanMemberRole>();
        modelBuilder.HasPostgresEnum<ClanInvitationType>();
        modelBuilder.HasPostgresEnum<ClanInvitationStatus>();
        modelBuilder.HasPostgresEnum<PartyStatus>();
        modelBuilder.HasPostgresEnum<SettlementType>();
        modelBuilder.HasPostgresEnum<BattlePhase>();
        modelBuilder.HasPostgresEnum<BattleSide>();
        modelBuilder.HasPostgresEnum<BattleFighterApplicationStatus>();
        modelBuilder.HasPostgresEnum<BattleMercenaryApplicationStatus>();
        modelBuilder.HasPostgresEnum<Region>();
        modelBuilder.HasPostgresEnum<ActivityLogType>();

        // Ensure that the PostGIS extension is installed.
        modelBuilder.HasPostgresExtension("postgis");
    }
}
