using Hotel_Booking_API.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_API.Infrastructure.Data.CompiledQueries
{
    internal static class BookingCompiledQueries
    {
        // ==============================
        // COUNT QUERIES (Optimized)
        // ==============================

        private static readonly Func<ApplicationDbContext, int, Task<int>> CountBookingsByUserQuery =
            EF.CompileAsyncQuery((ApplicationDbContext context, int userId) =>
                context.Bookings
                    .AsNoTracking()
                    .Count(b => b.UserId == userId && !b.IsDeleted));

        private static readonly Func<ApplicationDbContext, int, Task<int>> CountBookingsByHotelQuery =
            EF.CompileAsyncQuery((ApplicationDbContext context, int hotelId) =>
                context.Bookings
                    .AsNoTracking()
                    .Count(b => b.Room.HotelId == hotelId && !b.IsDeleted));



        // ==============================
        // PAGINATION QUERIES (Manual Select)
        // ==============================

        private static readonly Func<ApplicationDbContext, int, int, int, IAsyncEnumerable<BookingsForUserDto>>
            BookingsByUserPageQuery =
                EF.CompileAsyncQuery(
                    (ApplicationDbContext context,
                     int userId,
                     int skip,
                     int take) =>

                        (from b in context.Bookings
                         join r in context.Rooms on b.RoomId equals r.Id
                         join h in context.Hotels on r.HotelId equals h.Id
                         join u in context.Users on b.UserId equals u.Id
                         join p in context.Payments on b.Id equals p.BookingId into payGroup
                         from payment in payGroup.DefaultIfEmpty()

                         where b.UserId == userId && !b.IsDeleted

                         orderby b.CreatedAt descending

                         select new BookingsForUserDto
                         {
                             Id = b.Id,
                             UserId = b.UserId,
                             UserName = u.FirstName + " " + u.LastName,

                             RoomNumber = r.RoomNumber,
                             HotelName = h.Name,

                             CheckInDate = b.CheckInDate,
                             CheckOutDate = b.CheckOutDate,
                             TotalPrice = b.TotalPrice,
                             Status = b.Status,
                             CreatedAt = b.CreatedAt,

                             Payment = payment == null ? null : new PaymentDto
                             {
                                 Id = payment.Id,
                                 Amount = payment.Amount,
                                 PaymentMethod = payment.PaymentMethod,
                                 Status = payment.Status,
                                 CreatedAt = payment.CreatedAt
                             }
                         })
                         .Skip(skip)
                         .Take(take)
                );

        private static readonly Func<ApplicationDbContext, int, int, int, IAsyncEnumerable<BookingsForHotelDto>>
            BookingsByHotelPageQuery =
                EF.CompileAsyncQuery(
                    (ApplicationDbContext context,
                     int hotelId,
                     int skip,
                     int take) =>

                        (from b in context.Bookings
                         join r in context.Rooms on b.RoomId equals r.Id
                         join h in context.Hotels on r.HotelId equals h.Id
                         join u in context.Users on b.UserId equals u.Id
                         join p in context.Payments on b.Id equals p.BookingId into payGroup
                         from payment in payGroup.DefaultIfEmpty()

                         where h.Id == hotelId && !b.IsDeleted

                         orderby b.CreatedAt descending

                         select new BookingsForHotelDto
                         {
                             Id = b.Id,
                             UserName = u.FirstName + " " + u.LastName,
                             RoomNumber = r.RoomNumber,
                             HotelName = h.Name,

                             CheckInDate = b.CheckInDate,
                             CheckOutDate = b.CheckOutDate,
                             TotalPrice = b.TotalPrice,
                             Status = b.Status,
                             CreatedAt = b.CreatedAt,
                             Payment = b.Payment == null ? null : new PaymentDto
                             {
                                 Id = b.Payment.Id,
                                 Amount = b.Payment.Amount,
                                 PaymentMethod = b.Payment.PaymentMethod,
                                 Status = b.Payment.Status,
                                 CreatedAt = b.Payment.CreatedAt
                             }
                         })
                         .Skip(skip)
                         .Take(take)
                );




        // ==============================
        // PUBLIC METHODS
        // ==============================

        public static Task<int> CountBookingsByUserAsync(
            ApplicationDbContext context,
            int userId,
            CancellationToken cancellationToken = default)
            => CountBookingsByUserQuery(context, userId);

        public static Task<int> CountBookingsByHotelAsync(
            ApplicationDbContext context,
            int hotelId,
            CancellationToken cancellationToken = default)
            => CountBookingsByHotelQuery(context, hotelId);



        public static async Task<List<BookingsForUserDto>> GetBookingsByUserPageAsync(
            ApplicationDbContext context,
            int userId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var result = new List<BookingsForUserDto>();

            await foreach (var item in BookingsByUserPageQuery(context, userId, skip, take)
                               .WithCancellation(cancellationToken))
            {
                result.Add(item);
            }

            return result;
        }


        public static async Task<List<BookingsForHotelDto>> GetBookingsByHotelPageAsync(
            ApplicationDbContext context,
            int hotelId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var result = new List<BookingsForHotelDto>();

            await foreach (var dto in BookingsByHotelPageQuery(context, hotelId, skip, take)
                                   .WithCancellation(cancellationToken))
            {
                result.Add(dto);
            }

            return result;
        }
    }
}
