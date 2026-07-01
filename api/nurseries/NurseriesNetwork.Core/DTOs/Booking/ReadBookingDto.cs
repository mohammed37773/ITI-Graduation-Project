using NurseriesNetwork.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Booking
{
    public class ReadBookingDto
    {
        public int Id { get; set; }
        public int NurseryId { get; set; }
        public string NurseryName { get; set; } = string.Empty;
        public int ChildId { get; set; }
        public string ChildName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public string? Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
