using System.Threading;
using System.Threading.Tasks;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crpg.Application.Common.Interfaces
{
    public interface ICrpgDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Character> Characters { get; }
        DbSet<Item> Items { get; }
        DbSet<UserItem> UserItems { get; }
        DbSet<EquippedItem> EquippedItems { get; }
        DbSet<Ban> Bans { get; }
        DbSet<Clan> Clans { get; }
        DbSet<ClanMember> ClanMembers { get; }
        DbSet<ClanInvitation> ClanInvitations { get; }
        DbSet<Hero> Heroes { get; }
        DbSet<Settlement> Settlements { get; }
        DbSet<SettlementItem> SettlementItems { get; }
        DbSet<HeroItem> HeroItems { get; }
        DbSet<Battle> Battles { get; }
        DbSet<BattleFighter> BattleFighters { get; }
        DbSet<FighterApplication> BattleFighterApplications { get; }
        DbSet<BattleMercenary> BattleMercenaries { get; }
        DbSet<BattleMercenaryApplication> BattleMercenaryApplications { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
