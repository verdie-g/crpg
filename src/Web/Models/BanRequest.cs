namespace Crpg.Web.Models
{
    public class BanRequest
    {
        /// <summary>
        /// Ban duration in seconds. 0 to unban.
        /// </summary>
        public int Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}