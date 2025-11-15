using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_API.Infrastructure.Repositories
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoomRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsRoomAvailableAsync(
            int roomId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return !await _dbContext.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.RoomId == roomId &&
                    !b.IsDeleted &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed &&
                    b.CheckInDate < endDate &&    // Overlap logic
                    b.CheckOutDate > startDate
                )
                .AnyAsync(cancellationToken);
        }

    }
}
