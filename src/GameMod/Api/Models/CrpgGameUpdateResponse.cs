using System;
using System.Collections.Generic;
using Crpg.GameMod.Api.Models.Users;

namespace Crpg.GameMod.Api.Models
{
    // Copy of Crpg.Application.Games.Models.UpdateGameResult
    internal class CrpgGameUpdateResponse
    {
        public IList<CrpgUser> Users { get; set; } = new List<CrpgUser>();
    }
}