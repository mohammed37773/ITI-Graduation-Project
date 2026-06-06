using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class NurseryImage
    {
        public int Id { get; set; }
        public int NurseryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Nursery Nursery { get; set; } = null!;
    }
}
