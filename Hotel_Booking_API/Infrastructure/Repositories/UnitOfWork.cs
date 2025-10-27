using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Data;

namespace Hotel_Booking_API.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<User>? _users;
        private IRepository<Hotel>? _hotels;
        private IRepository<Room>? _rooms;
        private IRepository<Booking>? _bookings;
        private IRepository<Review>? _reviews;
        private IRepository<Payment>? _payments;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Hotel> Hotels => _hotels ??= new Repository<Hotel>(_context);
        public IRepository<Room> Rooms => _rooms ??= new Repository<Room>(_context);
        public IRepository<Booking> Bookings => _bookings ??= new Repository<Booking>(_context);
        public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);
        public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
