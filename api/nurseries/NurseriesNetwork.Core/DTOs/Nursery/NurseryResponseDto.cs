using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record NurseryResponseDto(
    int Id,
    string Name,
    string Description,
    decimal DailyPrice,
    int AgeRangeMin,
    int AgeRangeMax,
    double AvgRating,
    bool IsVerified,
    string City,
    string Address,
    double Latitude,
    double Longitude,
    double? DistanceKm,             // بيتحسب في الـ Nearby endpoint
    List<string> ImageUrls
);
}
