using System;

namespace Crpg.Domain.Common
{
    public class AuditableEntity
    {
        public DateTime LastModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}