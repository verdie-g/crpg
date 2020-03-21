using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Users
{
    public class UserViewModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public long SteamId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Gold { get; set; }
        public Role Role { get; set; }
        public Uri? AvatarSmall { get; set; }
        public Uri? AvatarMedium { get; set; }
        public Uri? AvatarFull { get; set; }
    }
}