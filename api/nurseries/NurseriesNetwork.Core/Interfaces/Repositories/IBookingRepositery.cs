using NurseriesNetwork.Core.DTOs.Booking;
using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Repositories
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<List<ReadBookingDto>?> GetWithDetailsAsync(string parentId);
        Task<List<ReadBookingDto>?> GetNurseryBookingsWithDetailsAsync(string ownerId);
        Task<decimal?> CompleteBookingAsync(int bookingId);
    }
}
