using System;
using System.Collections.Generic;

namespace Crpg.GameMod.Api.Requests
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Commands.TickCommand.
    /// </summary>
    public class TickRequest
    {
        public IReadOnlyList<UserTick> Users { get; set; } = Array.Empty<UserTick>();
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.UserTick.
    /// </summary>
    public class UserTick
    {
        public int CharacterId { get; set; }
        public int ExperienceGain { get; set; }
        public int GoldGain { get; set; }
    }
}