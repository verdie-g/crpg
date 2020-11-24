using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models
{
    public class UserPublicViewModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}