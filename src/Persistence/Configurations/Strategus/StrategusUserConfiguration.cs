using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusUserConfiguration : IEntityTypeConfiguration<StrategusUser>
    {
        public void Configure(EntityTypeBuilder<StrategusUser> builder)
        {
            builder.HasKey(u => u.UserId);
        }
    }
}
