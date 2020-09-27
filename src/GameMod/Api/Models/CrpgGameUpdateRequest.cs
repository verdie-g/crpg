using System;
using System.Collections.Generic;

namespace Crpg.GameMod.Api.Models
{
    // Copy of Crpg.Application.Games.Commands.UpdateGameCommand
    internal class CrpgGameUpdateRequest
    {
        public IList<CrpgGameUserUpdate> GameUserUpdates { get; set; } = Array.Empty<CrpgGameUserUpdate>();
    }
}