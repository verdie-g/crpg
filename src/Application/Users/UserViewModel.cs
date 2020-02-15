using System;
using Trpg.Application.Common.Mappings;
using Trpg.Domain.Entities;

namespace Trpg.Application.Users
{
    public class UserViewModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public long SteamId { get; set; }
        public string UserName { get; set; }
        public int Money { get; set; }
        public Role Role { get; set; }
        public Uri AvatarSmall { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
    }
}