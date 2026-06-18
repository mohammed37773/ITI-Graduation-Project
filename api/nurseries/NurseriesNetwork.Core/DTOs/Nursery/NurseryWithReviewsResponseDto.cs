using NurseriesNetwork.Core.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public class NurseryWithReviewsResponseDto: NurseryResponseDto
    {
        public List<ReviewResponseDto> Reviews { get; init; } = new();
    }
}
