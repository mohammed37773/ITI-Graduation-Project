using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Booking
{
    public record CreateBookingDto(
     [Required] int NurseryId,
     [Required] int ChildId,
     [Required] DateOnly StartDate
 );
}
