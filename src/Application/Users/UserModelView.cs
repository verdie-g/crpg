using System;
using Trpg.Application.Common.Mappings;
using Trpg.Domain.Entities;

namespace Trpg.Application.Users
{
    public class UserModelView : IMapFrom<User>
    {
        public int Id { get; set; }
        public string SteamId { get; set; }
        public string UserName { get; set; }
        public int Money { get; set; }
        public Role Role { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
    }
}