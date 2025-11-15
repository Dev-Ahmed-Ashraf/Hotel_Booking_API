using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hotel_Booking_API.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Generic repos
        private IRepository<User>? _users;
        private IRepository<Hotel>? _hotels;
        private IRepository<Booking>? _bookings;
        private IRepository<Review>? _reviews;
        private IRepository<Payment>? _payments;

        // Custom repos
        private IRoomRepository? _rooms;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Repositories
        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Hotel> Hotels => _hotels ??= new Repository<Hotel>(_context);
        public IRepository<Booking> Bookings => _bookings ??= new Repository<Booking>(_context);
        public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);
        public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);

        public IRoomRepository Rooms => _rooms ??= new RoomRepository(_context);

        // ----------- Transaction Support -----------

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
