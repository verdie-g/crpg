using System;

namespace Trpg.Domain.Common
{
    public class AuditableEntity
    {
        public DateTime LastModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}