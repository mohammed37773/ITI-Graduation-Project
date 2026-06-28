using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public class NurseryFilterDto
    {

        [Range(1, 10000)] decimal? MaxPrice;
        [Range(1, 5)] double? MinRating;
        string? City;
        [Range(0, 144)] int? AgeInMonths;
        [Range(1, 100)] int PageNumber = 1;
        [Range(1, 50)] int PageSize = 10;




    }
}
