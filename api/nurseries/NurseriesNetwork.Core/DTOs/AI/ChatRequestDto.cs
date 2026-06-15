using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.AI
{
    public record ChatRequestDto(
    string Message,
    double? Latitude,
    double? Longitude
);
}
