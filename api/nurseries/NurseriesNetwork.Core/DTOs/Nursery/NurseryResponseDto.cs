using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Nursery
{
    public class NurseryResponseDto
    {

        int Id;
        string Name;
    string Description;
    decimal DailyPrice;
    int AgeRangeMin;
    int AgeRangeMax;
    double AvgRating;
    bool IsVerified;
    string City;
    string Address;
    double Latitude;
    double Longitude;
    double? DistanceKm;
        List<string> ImageUrls;
    }
}
