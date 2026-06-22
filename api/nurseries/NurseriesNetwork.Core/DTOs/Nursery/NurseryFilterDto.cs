using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> main
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public record NurseryFilterDto(
<<<<<<< HEAD
    decimal? MaxPrice,
    double? MinRating,
    string? City,
    int? AgeInMonths,               // عشان نفلتر على AgeRange
    int PageNumber = 1,
    int PageSize = 10
=======
    [Range(1, 10000)] decimal? MaxPrice,
    [Range(1, 5)] double? MinRating,
    string? City,
    [Range(0, 144)] int? AgeInMonths,
    [Range(1, 100)] int PageNumber = 1,
    [Range(1, 50)] int PageSize = 10
>>>>>>> main
);
}
