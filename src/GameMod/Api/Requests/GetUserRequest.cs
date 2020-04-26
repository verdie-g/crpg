namespace Crpg.GameMod.Api.Requests
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Commands.UpsertGameUserCommand.
    /// </summary>
    public class GetUserRequest
    {
        public ulong SteamId { get; set; }
        public string CharacterName { get; set; } = default!;
    }
}