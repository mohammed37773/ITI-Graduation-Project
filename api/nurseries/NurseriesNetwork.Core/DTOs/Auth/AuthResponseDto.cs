

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }

        public IEnumerable<string>? Errors { get; set; }

        public string? Token { get; set; }
    }
}
