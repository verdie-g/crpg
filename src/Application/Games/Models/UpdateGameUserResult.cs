using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class UpdateGameUserResult
    {
        public GameUser User { get; set; } = default!;
        public IList<GameUserBrokenItem> BrokenItems { get; set; } = Array.Empty<GameUserBrokenItem>();
    }
}
