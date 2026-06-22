using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> main
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record CreateNurseryDto(
<<<<<<< HEAD
     string Name,
     string Description,
     decimal DailyPrice,
     int AgeRangeMin,
     int AgeRangeMax,
     int Capacity,
     string Address,
     string City,
     string District,
     double Latitude,
     double Longitude
 );
=======
    [Required][MaxLength(200)] string Name,
    [Required] string Description,
    [Required][Range(1, 10000)] decimal DailyPrice,
    [Required][Range(0, 12)] int AgeRangeMin,
    [Required][Range(1, 12)] int AgeRangeMax,
    [Required][Range(1, 500)] int Capacity,
    [Required] string Address,
    [Required] string City,
    [Required] string District,
    [Required][Range(-90, 90)] double Latitude,
    [Required][Range(-180, 180)] double Longitude
);
>>>>>>> main
}
