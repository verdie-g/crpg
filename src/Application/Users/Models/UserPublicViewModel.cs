using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Users.Models
{
    public class UserPublicViewModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public string PlatformId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}