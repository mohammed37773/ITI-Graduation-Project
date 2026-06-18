using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Rating { get; init; }
        public string Comment { get; init; } = string.Empty;
        public string ParentId { get; init; } = string.Empty;
        public string ParentName { get; init; } = string.Empty;
    }
}
