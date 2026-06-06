using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class Child
    {
        public int Id { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string? SpecialNeeds { get; set; }

        // Navigation Properties
        public ApplicationUser Parent { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
