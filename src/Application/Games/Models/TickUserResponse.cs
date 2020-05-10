namespace Crpg.Application.Games.Models
{
    public class TickUserResponse
    {
        public int UserId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Gold { get; set; }
    }
}