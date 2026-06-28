using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



using System.ComponentModel.DataAnnotations;

using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public class CreateNurseryDto
    {
        [Required][MaxLength(200)] string Name;
        [Required] string Description;
        [Required][Range(1, 10000)] decimal DailyPrice;
        [Required][Range(0, 12)] int AgeRangeMin;
        [Required][Range(1, 12)] int AgeRangeMax;
        [Required][Range(1, 500)] int Capacity;
        [Required] string Address;
        [Required] string City;
        [Required] string District;
        [Required][Range(-90, 90)] double Latitude;
        [Required][Range(-180, 180)] double Longitude;

    }

}
