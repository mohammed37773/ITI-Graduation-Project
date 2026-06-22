<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record AuthResponseDto(
    string Token,
    string FullName,
    string Email,
    string Role
);
=======
﻿

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }

        public IEnumerable<string>? Errors { get; set; }

        public string? Token { get; set; }

        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? NameIdentifier { get; set; }
    }
>>>>>>> main
}
