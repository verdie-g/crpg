using System;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models
{
    public record UserViewModel : UserPublicViewModel
    {
        public int Gold { get; init; }
        public int HeirloomPoints { get; init; }
        public Role Role { get; init; }
        public Uri? AvatarSmall { get; init; }
        public Uri? AvatarMedium { get; init; }
        public Uri? AvatarFull { get; init; }
    }
}
