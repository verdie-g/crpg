using System;
using Trpg.Domain.Common;

namespace Trpg.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public string SteamId { get; set; }
        public string UserName { get; set; }
        public Role Role { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
    }
}