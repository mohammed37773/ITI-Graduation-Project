using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class Nursery
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public int AgeRangeMin { get; set; }
        public int AgeRangeMax { get; set; }
        public int Capacity { get; set; }
        public double AvgRating { get; set; } = 0.0;
        public bool IsVerified { get; set; } = false;
        public byte[]? EmbeddingVector { get; set; }      // للـ RAG
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Location? Location { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<NurseryImage> Images { get; set; } = new List<NurseryImage>();
    }
}
