using System.Collections.Generic;

namespace Crpg.Web.Models
{
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Details { get; set; }
    }
}