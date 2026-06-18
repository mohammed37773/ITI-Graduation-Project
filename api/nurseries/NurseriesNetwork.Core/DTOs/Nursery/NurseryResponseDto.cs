using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public class NurseryResponseDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal DailyPrice { get; init; }
        public int AgeRangeMin { get; init; }
        public int AgeRangeMax { get; init; }
        public double AvgRating { get; init; }
        public bool IsVerified { get; init; }
        public string City { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public List<string> ImageUrls { get; init; } = new();
    }
}