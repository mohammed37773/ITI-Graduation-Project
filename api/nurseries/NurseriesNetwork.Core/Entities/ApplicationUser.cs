using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public double? LocationLat { get; set; }
        public double? LocationLng { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Child> Children { get; set; } = new List<Child>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
