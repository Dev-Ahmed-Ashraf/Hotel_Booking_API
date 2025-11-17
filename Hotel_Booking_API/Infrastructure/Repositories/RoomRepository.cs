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
            int? excludeBookingId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.RoomId == roomId &&
                    !b.IsDeleted &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed
                );

            // Exclude current booking to avoid self-conflict
            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            // Overlap logic
            query = query.Where(b =>
                b.CheckInDate < endDate &&
                b.CheckOutDate > startDate
            );

            return !await query.AnyAsync(cancellationToken);
        }


    }
}
