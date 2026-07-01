using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Review
{
    public class ReadReviewsDto
    {
        public string ParentName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
