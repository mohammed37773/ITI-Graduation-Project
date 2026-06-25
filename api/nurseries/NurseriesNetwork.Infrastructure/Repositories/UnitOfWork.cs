using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;

namespace NurseriesNetwork.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public INurseryRepository Nurseries { get; }
    public IGenericRepository<Review> Reviews { get; }
    public IGenericRepository<Booking> Bookings { get; }
    public IGenericRepository<Child> Children { get; }
    public IGenericRepository<NurseryImage> NurseryImages { get; }
    public IGenericRepository<Payment> Payments { get; }
    public IGenericRepository<ApplicationUser> Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Nurseries = new NurseryRepository(context);
        Reviews = new GenericRepository<Review>(context);
        Bookings = new GenericRepository<Booking>(context);
        Children = new GenericRepository<Child>(context);
        NurseryImages = new GenericRepository<NurseryImage>(context);
        Payments = new GenericRepository<Payment>(context);
        Users = new GenericRepository<ApplicationUser>(context);
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