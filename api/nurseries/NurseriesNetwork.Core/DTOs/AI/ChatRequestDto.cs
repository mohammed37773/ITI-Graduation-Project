using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> main
using System.Text;

namespace NurseriesNetwork.Core.DTOs.AI
{
    public record ChatRequestDto(
<<<<<<< HEAD
    string Message,
    double? Latitude,
    double? Longitude
);
=======
     [Required][MaxLength(500)] string Message,
     [Range(-90, 90)] double? Latitude,
     [Range(-180, 180)] double? Longitude
 );
>>>>>>> main
}
