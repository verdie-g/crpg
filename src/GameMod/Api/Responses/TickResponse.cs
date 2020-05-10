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
    /// Copy of Crpg.Application.Games.Models.
    /// </summary>
    public class TickUserResponse
    {
        public int UserId { get; set; }

        /// <summary>
        /// Increased if the user leveled up of the tick.
        /// </summary>
        public int Level { get; set; }
        public int NextLevelExperience { get; set; }
    }
}