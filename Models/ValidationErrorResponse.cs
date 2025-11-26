using System.Collections.Generic;

namespace Ordenes.Api.Models
{
    public sealed class ValidationErrorResponse
    {
        public string TraceId { get; set; } = string.Empty;

        public Dictionary<string, List<string>> Errors { get; set; } = new Dictionary<string, List<string>>();
    }
}
