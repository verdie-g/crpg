namespace Crpg.WebApi.Models
{
    public class BanRequest
    {
        /// <summary>
        /// Ban duration in milliseconds. 0 to unban.
        /// </summary>
        public int Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}