using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public record UpdateGameUsersResult
    {
        public IList<UpdateGameUserResult> UpdateResults { get; init; } = new List<UpdateGameUserResult>();
    }
}
