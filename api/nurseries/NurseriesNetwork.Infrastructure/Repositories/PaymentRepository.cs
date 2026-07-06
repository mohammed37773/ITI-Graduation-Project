using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.DTOs.Payment;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {

        public PaymentRepository(AppDbContext context) : base(context) { }

        
        public async Task<Payment?> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

    }
}
