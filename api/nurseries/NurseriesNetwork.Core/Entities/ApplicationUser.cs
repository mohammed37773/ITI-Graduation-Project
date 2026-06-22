using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
<<<<<<< HEAD
    public class ApplicationUser: IdentityUser
=======
    public class ApplicationUser : IdentityUser
>>>>>>> main
    {
        public string FullName { get; set; } = string.Empty;
        public double? LocationLat { get; set; }
        public double? LocationLng { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

<<<<<<< HEAD
        // Navigation Properties
        public ICollection<Child> Children { get; set; } = new List<Child>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
=======
        public string? Otp { get; set; }
        public DateTime? OtpExpiryTime { get; set; }

        public ICollection<Child> Children { get; set; } = new List<Child>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
>>>>>>> main
    }
}
