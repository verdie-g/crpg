using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Trpg.Domain.Entities;

namespace Trpg.Application.Common.Interfaces
{
    public interface ITrpgDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Character> Characters { get; }
        DbSet<Item> Items { get; }
        DbSet<UserItem> UserItems { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}