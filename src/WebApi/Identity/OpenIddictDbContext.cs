using Microsoft.EntityFrameworkCore;

namespace Crpg.WebApi.Identity;

internal class OpenIddictDbContext : DbContext
{
    public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options)
        : base(options)
    {
    }
}
