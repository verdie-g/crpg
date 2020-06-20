using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class UpdateGameResult
    {
        public IList<GameUser> Users { get; set; } = new List<GameUser>();
    }
}