using NurseriesNetwork.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public int NurseryId { get; set; }
        public int ChildId { get; set; }
        public DateOnly StartDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

<<<<<<< HEAD
        // Navigation Properties
        public ApplicationUser Parent { get; set; } = null!;
        public Nursery Nursery { get; set; } = null!;
        public Child Child { get; set; } = null!;
=======
        public ApplicationUser Parent { get; set; } = null!;
        public Nursery Nursery { get; set; } = null!;
        public Child Child { get; set; } = null!;
        public Payment? Payment { get; set; }
>>>>>>> main
    }
}
