using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record CreateNurseryDto(
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
}
