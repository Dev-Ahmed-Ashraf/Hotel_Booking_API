using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByUser
{
    /// <summary>
    /// Handler for retrieving all bookings for a specific user.
    /// Returns paginated list of user's bookings with related information.
    /// </summary>
    public class GetBookingsByUserQueryHandler : IRequestHandler<GetBookingsByUserQuery, ApiResponse<PagedList<BookingDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetBookingsByUserQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the get bookings by user query by retrieving user's bookings with pagination.
        /// </summary>
        /// <param name="request">The query containing user ID and pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing paginated list of user's bookings or error message</returns>
        public async Task<ApiResponse<PagedList<BookingDto>>> Handle(GetBookingsByUserQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingsByUserQueryHandler), request);

            try
            {
                // Start with base query for user's bookings
                IQueryable<Booking> query = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .Include(b => b.Room!.Hotel)
                    .Include(b => b.Payment)
                    .AsNoTracking()
                    .Where(b => b.UserId == request.UserId && !b.IsDeleted);

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and ordering
                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(b => new BookingDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        UserName = $"{b.User!.FirstName} {b.User.LastName}",
                        RoomId = b.RoomId,
                        RoomNumber = b.Room!.RoomNumber,
                        HotelName = b.Room.Hotel!.Name,
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        TotalPrice = b.TotalPrice,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        Payment = b.Payment != null ? new PaymentDto
                        {
                            Id = b.Payment.Id,
                            Amount = b.Payment.Amount,
                            PaymentMethod = b.Payment.PaymentMethod,
                            Status = b.Payment.Status,
                            PaidAt = b.Payment.PaidAt,
                            CreatedAt = b.Payment.CreatedAt
                        } : null
                    })
                    .ToListAsync(cancellationToken);

                // Create paginated result
                var pagedList = new PagedList<BookingDto>(
                    bookings,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("User bookings retrieved successfully: {TotalCount} bookings found for user {UserId}, page {PageNumber}", totalCount, request.UserId, request.Pagination.PageNumber);
                Log.Information("Completed {HandlerName} successfully", nameof(GetBookingsByUserQueryHandler));

                return ApiResponse<PagedList<BookingDto>>.SuccessResponse(pagedList, "User bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingsByUserQueryHandler));
                throw;
            }
        }
    }
}
