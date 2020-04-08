using System;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities
{
    public class Ban : AuditableEntity
    {
        public int Id { get; set; }
        public int BannedUserId { get; set; }
        public DateTimeOffset Until { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int BannedByUserId { get; set; }

        public User? BannedUser { get; set; }
        public User? BannedByUser { get; set; }
    }
}