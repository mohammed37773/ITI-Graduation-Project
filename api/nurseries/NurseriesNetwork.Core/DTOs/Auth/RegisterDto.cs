<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string Role = "Parent"
);
=======
﻿using System.ComponentModel.DataAnnotations;
namespace NurseriesNetwork.Core.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full name is requierd !")]
        [MaxLength(20)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is requierd !"), EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is requierd !"), MinLength(6)]
        public string Password { get; set; } = null!;

        [Compare(nameof(Password), ErrorMessage = "The Password and Confirmation do not match !")]
        public string ConfirmPassword { get; set; } = null!;
    }
>>>>>>> main
}
