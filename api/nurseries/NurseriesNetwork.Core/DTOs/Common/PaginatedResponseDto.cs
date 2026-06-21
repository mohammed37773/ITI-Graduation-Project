// Core/DTOs/Common/PaginatedResponseDto.cs
namespace NurseriesNetwork.Core.DTOs.Common;

public record PaginatedResponseDto<T>(
    IEnumerable<T> Data,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);