using System;

namespace Crpg.Domain.Common
{
    public class AuditableEntity
    {
        public DateTimeOffset LastModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
