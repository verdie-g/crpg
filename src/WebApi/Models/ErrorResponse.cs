using System.Collections.Generic;

namespace Crpg.WebApi.Models
{
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Details { get; set; }
    }
}