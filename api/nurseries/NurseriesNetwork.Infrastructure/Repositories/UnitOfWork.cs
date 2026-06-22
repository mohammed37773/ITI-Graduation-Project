using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;

namespace NurseriesNetwork.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public INurseryRepository Nurseries { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<Booking> Bookings { get; }
        public IGenericRepository<Child> Children { get; }
        public IGenericRepository<NurseryImage> NurseryImages { get; }

<<<<<<< HEAD
=======
        public IGenericRepository<Payment> Payments => throw new NotImplementedException();

>>>>>>> main
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Nurseries = new NurseryRepository(context);
            Reviews = new GenericRepository<Review>(context);
            Bookings = new GenericRepository<Booking>(context);
            Children = new GenericRepository<Child>(context);
            NurseryImages = new GenericRepository<NurseryImage>(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}