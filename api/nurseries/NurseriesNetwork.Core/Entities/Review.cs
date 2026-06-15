using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public int NurseryId { get; set; }
        public int Rating { get; set; }          // 1 → 5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser Parent { get; set; } = null!;
        public Nursery Nursery { get; set; } = null!;
    }
}
