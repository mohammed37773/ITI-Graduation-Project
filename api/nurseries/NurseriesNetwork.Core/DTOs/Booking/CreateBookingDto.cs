using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Booking
{
    public record CreateBookingDto(
     int NurseryId,
     int ChildId,
     DateOnly StartDate
 );
}
