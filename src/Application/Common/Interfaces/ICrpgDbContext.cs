using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crpg.Application.Common.Interfaces;

public interface ICrpgDbContext
{
    DbSet<User> Users { get; }
    DbSet<Character> Characters { get; }
    DbSet<Item> Items { get; }
    DbSet<UserItem> UserItems { get; }
    DbSet<EquippedItem> EquippedItems { get; }
    DbSet<Restriction> Restrictions { get; }
    DbSet<Clan> Clans { get; }
    DbSet<ClanMember> ClanMembers { get; }
    DbSet<ClanInvitation> ClanInvitations { get; }
    DbSet<Party> Parties { get; }
    DbSet<Settlement> Settlements { get; }
    DbSet<SettlementItem> SettlementItems { get; }
    DbSet<PartyItem> PartyItems { get; }
    DbSet<Battle> Battles { get; }
    DbSet<BattleFighter> BattleFighters { get; }
    DbSet<BattleFighterApplication> BattleFighterApplications { get; }
    DbSet<BattleMercenary> BattleMercenaries { get; }
    DbSet<BattleMercenaryApplication> BattleMercenaryApplications { get; }
    DbSet<ActivityLog> ActivityLogs { get; set; }
    DbSet<ActivityLogMetadata> ActivityLogMetadata { get; set; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
