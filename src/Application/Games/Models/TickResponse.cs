using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class TickResponse
    {
        public IReadOnlyList<TickUserResponse> Users { get; set; } = Array.Empty<TickUserResponse>();
    }
}