using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        INurseryRepository Nurseries { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Child> Children { get; }
        IGenericRepository<NurseryImage> NurseryImages { get; }
        IGenericRepository<Payment> Payments { get; }
        Task<int> SaveChangesAsync();
    }
}
