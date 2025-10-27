using Hotel_Booking_API.Domain.Entities;

namespace Hotel_Booking_API.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Hotel> Hotels { get; }
        IRepository<Room> Rooms { get; }
        IRepository<Booking> Bookings { get; }
        IRepository<Review> Reviews { get; }
        IRepository<Payment> Payments { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
