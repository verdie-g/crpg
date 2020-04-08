using System;
using System.Collections.Generic;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public long SteamId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Gold { get; set; }
        public int LoomPoints { get; set; }
        public Role Role { get; set; }

        /// <summary>
        /// 32x32.
        /// </summary>
        public Uri? AvatarSmall { get; set; }

        /// <summary>
        /// 64x64.
        /// </summary>
        public Uri? AvatarMedium { get; set; }

        /// <summary>
        /// 184x184.
        /// </summary>
        public Uri? AvatarFull { get; set; }

        public List<UserItem> UserItems { get; set; } = new List<UserItem>();
        public List<Character> Characters { get; set; } = new List<Character>();
        public List<Ban> Bans { get; set; } = new List<Ban>();
    }
}