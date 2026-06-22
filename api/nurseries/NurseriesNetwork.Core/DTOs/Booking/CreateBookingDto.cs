using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> main
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Booking
{
    public record CreateBookingDto(
<<<<<<< HEAD
     int NurseryId,
     int ChildId,
     DateOnly StartDate
=======
     [Required] int NurseryId,
     [Required] int ChildId,
     [Required] DateOnly StartDate
>>>>>>> main
 );
}
