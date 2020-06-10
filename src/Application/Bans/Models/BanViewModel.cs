using System;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Bans.Models
{
    public class BanViewModel : IMapFrom<Ban>
    {
        public int Id { get; set; }
        public UserPublicViewModel BannedUser { get; set; } = default!;
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
        public UserPublicViewModel BannedByUser { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}