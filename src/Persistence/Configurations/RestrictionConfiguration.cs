using Crpg.Domain.Entities.Restrictions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class RestrictionConfiguration : IEntityTypeConfiguration<Restriction>
{
    public void Configure(EntityTypeBuilder<Restriction> builder)
    {
        builder
            .HasOne(r => r.RestrictedUser!)
            .WithMany(u => u.Restrictions);
    }
}
