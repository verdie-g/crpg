using System;
using System.Collections.Generic;
using Crpg.GameMod.Api.Models.Users;

namespace Crpg.GameMod.Api.Models
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Models.UpdateGameUserResult.
    /// </summary>
    internal class UpdateCrpgUserResult
    {
        public CrpgUser User { get; set; } = default!;
        public IList<CrpgUserBrokenItem> BrokenItems { get; set; } = Array.Empty<CrpgUserBrokenItem>();
    }
}
