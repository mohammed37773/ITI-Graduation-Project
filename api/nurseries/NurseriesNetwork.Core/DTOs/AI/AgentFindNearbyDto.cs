using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.AI;

public record AgentFindNearbyDto(
    double Latitude,
    double Longitude,
    double RadiusKm = 10,
    decimal? MaxPrice = null
);
