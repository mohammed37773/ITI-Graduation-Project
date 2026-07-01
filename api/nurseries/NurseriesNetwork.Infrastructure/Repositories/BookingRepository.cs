using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.DTOs.Booking;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<List<ReadBookingDto>?> GetWithDetailsAsync(string parentId)
        {
            return await _context.Bookings
                .Where(b => b.ParentId == parentId)
                .Select(b => new ReadBookingDto
                {
                    Id = b.Id,
                    NurseryId = b.NurseryId,
                    NurseryName = b.Nursery.Name,
                    ChildId = b.ChildId,
                    ChildName = b.Child.FullName,
                    StartDate = b.StartDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                }).ToListAsync();
        }

        public async Task<List<ReadBookingDto>?> GetNurseryBookingsWithDetailsAsync(string ownerId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Nursery.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new ReadBookingDto
                {
                    Id = b.Id,
                    NurseryId = b.NurseryId,
                    NurseryName = b.Nursery.Name,
                    ChildId = b.ChildId,
                    ChildName = b.Child.FullName,
                    StartDate = b.StartDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }
    }
}
