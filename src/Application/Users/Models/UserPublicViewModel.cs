using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Users.Models
{
    public class UserPublicViewModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public long SteamId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}