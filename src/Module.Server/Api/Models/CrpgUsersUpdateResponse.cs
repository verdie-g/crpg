namespace Crpg.Module.Api.Models;

// Copy of Crpg.Application.Games.Models.UpdateGameUsersUpdateResult
internal class CrpgUsersUpdateResponse
{
    public IList<UpdateCrpgUserResult> UpdateResults { get; set; } = new List<UpdateCrpgUserResult>();
}
