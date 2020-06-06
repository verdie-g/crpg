using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Games.Models
{
    public class GameBan : IMapFrom<Ban>
    {
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}