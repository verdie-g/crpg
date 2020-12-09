using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models
{
    public class UserViewModel : UserPublicViewModel
    {
        public int Gold { get; set; }
        public int HeirloomPoints { get; set; }
        public Role Role { get; set; }
        public Uri? AvatarSmall { get; set; }
        public Uri? AvatarMedium { get; set; }
        public Uri? AvatarFull { get; set; }
    }
}
