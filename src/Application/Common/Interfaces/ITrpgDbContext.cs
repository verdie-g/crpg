using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trpg.Domain.Entities;

namespace Trpg.Application.Common.Interfaces
{
    public interface ITrpgDbContext
    {
        DbSet<User> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}