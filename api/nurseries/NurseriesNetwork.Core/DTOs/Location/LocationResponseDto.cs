using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Location
{
    public record LocationResponseDto(
    [Required][MaxLength(200)] string Address,
    [Required] string City,
    [Required] string District,
    [Required][Range(-90, 90)] double Latitude,
    [Required][Range(-180, 180)] double Longitude
    ); 
}


