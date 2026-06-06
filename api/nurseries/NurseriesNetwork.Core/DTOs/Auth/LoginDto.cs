using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record LoginDto(
     [Required][EmailAddress] string Email,
     [Required] string Password
 );

}
