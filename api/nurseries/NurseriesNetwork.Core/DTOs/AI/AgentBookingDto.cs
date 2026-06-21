using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.AI;

public record AgentBookingDto(
    int NurseryId,
    int ChildId,
    DateOnly StartDate
);