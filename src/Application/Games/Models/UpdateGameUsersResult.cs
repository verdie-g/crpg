using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class UpdateGameUsersResult
    {
        public IList<UpdateGameUserResult> UpdateResults { get; set; } = new List<UpdateGameUserResult>();
    }
}
