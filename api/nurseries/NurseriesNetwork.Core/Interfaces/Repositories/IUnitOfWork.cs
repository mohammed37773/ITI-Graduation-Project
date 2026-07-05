using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        INurseryRepository Nurseries { get; }
        IReviewRepository Reviews { get; }
        IBookingRepository Bookings { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Child> Children { get; }
        IGenericRepository<NurseryImage> NurseryImages { get; }
        IGenericRepository<ApplicationUser> Users { get; }
        Task<int> SaveChangesAsync();
    }
}
