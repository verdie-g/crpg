using System;
using System.Collections.Generic;
using Trpg.Domain.Common;

namespace Trpg.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public long SteamId { get; set; }
        public string UserName { get; set; }
        public int Money { get; set; }
        public Role Role { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
        // TODO: loom points + retirement

        public List<UserEquipment> UserEquipments { get; set; }
        public List<Character> Characters { get; set; }
    }
}