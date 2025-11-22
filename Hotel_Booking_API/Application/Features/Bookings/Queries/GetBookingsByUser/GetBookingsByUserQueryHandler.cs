using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using Hotel_Booking_API.Infrastructure.Data.CompiledQueries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByUser
{
    /// <summary>
    /// Handler for retrieving all bookings for a specific user.
    /// Returns paginated list of user's bookings with related information.
    /// </summary>
    public class GetBookingsByUserQueryHandler : IRequestHandler<GetBookingsByUserQuery, ApiResponse<PagedList<BookingsForUserDto>>>
    {
        private readonly ApplicationDbContext _context;

        public GetBookingsByUserQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedList<BookingsForUserDto>>> Handle(GetBookingsByUserQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingsByUserQueryHandler), request);

            try
            {
                // Check if user exists
                var userExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == request.UserId, cancellationToken);

                if (!userExists)
                {
                    Log.Warning("User with ID {UserId} does not exist.", request.UserId);
                    throw new NotFoundException("User", request.UserId);
                }

                var skip = (request.Pagination.PageNumber - 1) * request.Pagination.PageSize;

                // Use compiled queries for repeated access pattern
                var totalCount = await BookingCompiledQueries.CountBookingsByUserAsync(
                    _context,
                    request.UserId,
                    cancellationToken);

                var bookings = await BookingCompiledQueries.GetBookingsByUserPageAsync(
                    _context,
                    request.UserId,
                    skip,
                    request.Pagination.PageSize,
                    cancellationToken);

                // Create paginated result
                var pagedList = new PagedList<BookingsForUserDto>(
                    bookings,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("User bookings retrieved successfully: {TotalCount} bookings found for user {UserId}, page {PageNumber}", totalCount, request.UserId, request.Pagination.PageNumber);

                return ApiResponse<PagedList<BookingsForUserDto>>.SuccessResponse(pagedList, "User bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingsByUserQueryHandler));
                throw;
            }
        }
    }
}
