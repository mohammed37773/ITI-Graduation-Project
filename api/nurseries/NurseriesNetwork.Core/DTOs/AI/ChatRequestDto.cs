using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.AI
{
    public record ChatRequestDto(
     [Required][MaxLength(500)] string Message,
     [Range(-90, 90)] double? Latitude,
     [Range(-180, 180)] double? Longitude
 );
}
