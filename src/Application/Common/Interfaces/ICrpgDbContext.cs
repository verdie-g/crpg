using System.Threading;
using System.Threading.Tasks;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
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
        DbSet<OwnedItem> OwnedItems { get; }
        DbSet<EquippedItem> EquippedItems { get; }
        DbSet<Ban> Bans { get; }
        DbSet<Clan> Clans { get; }
        DbSet<ClanMember> ClanMembers { get; }
        DbSet<ClanInvitation> ClanInvitations { get; }
        DbSet<StrategusHero> StrategusHeroes { get; }
        DbSet<StrategusSettlement> StrategusSettlements { get; }
        DbSet<StrategusOwnedItem> StrategusOwnedItems { get; }
        DbSet<StrategusBattle> StrategusBattles { get; }
        DbSet<StrategusBattleFighter> StrategusBattleFighters { get; }
        DbSet<StrategusBattleFighterApplication> StrategusBattleFighterApplications { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
