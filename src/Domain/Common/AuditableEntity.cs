using System;

namespace Crpg.Domain.Common
{
    public class AuditableEntity
    {
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
