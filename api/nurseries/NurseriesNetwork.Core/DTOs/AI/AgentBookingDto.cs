namespace NurseriesNetwork.Core.DTOs.AI;

public record AgentBookingDto(
    int NurseryId,
    int ChildId,
    DateOnly StartDate
);