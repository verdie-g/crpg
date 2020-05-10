using System;
using System.Collections.Generic;

namespace Crpg.GameMod.Api.Responses
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Models.TickResponse.
    /// </summary>
    public class TickResponse
    {
        public IList<TickUserResponse> Users { get; set; } = Array.Empty<TickUserResponse>();
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.TickUserResponse.
    /// </summary>
    public class TickUserResponse
    {
        public int UserId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Gold { get; set; }
    }
}