using System;
using Crpg.Application.Users.Models;

namespace Crpg.WebApi.Models
{
    public class BanResponse
    {
        public int Id { get; set; }
        public UserPublicViewModel? BannedUser { get; set; }
        public int Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
        public UserPublicViewModel BannedByUser { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}