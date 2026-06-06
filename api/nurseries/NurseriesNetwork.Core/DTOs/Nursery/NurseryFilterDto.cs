using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record NurseryFilterDto(
    decimal? MaxPrice,
    double? MinRating,
    string? City,
    int? AgeInMonths,               // عشان نفلتر على AgeRange
    int PageNumber = 1,
    int PageSize = 10
);
}
