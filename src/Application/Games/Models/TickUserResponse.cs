namespace Crpg.Application.Games.Models
{
    public class TickUserResponse
    {
        public int UserId { get; set; }
        /// <summary>
        /// Increased if the user leveled up of the tick.
        /// </summary>
        public int Level { get; set; }
        public int NextLevelExperience { get; set; }
    }
}