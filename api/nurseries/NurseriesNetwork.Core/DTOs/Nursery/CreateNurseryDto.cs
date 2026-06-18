using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record CreateNurseryDto(
    [Required][MaxLength(200)] string Name,
    [Required] string Description,
    [Required][Range(1, int.MaxValue)] decimal DailyPrice,
    [Required][Range(0, 12)] int AgeRangeMin,
    [Required][Range(1, 12)] int AgeRangeMax,
    [Required][Range(1, 1000)] int Capacity,
    [Required] string Address,
    [Required] string City,
    [Required][Range(-90, 90)] double Latitude,
    [Required][Range(-180, 180)] double Longitude
);
}
