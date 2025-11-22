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

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByHotel
{
    public class GetBookingsByHotelQueryHandler : IRequestHandler<GetBookingsByHotelQuery, ApiResponse<PagedList<BookingsForHotelDto>>>
    {
        private readonly ApplicationDbContext _dbContext;

        public GetBookingsByHotelQueryHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<PagedList<BookingsForHotelDto>>> Handle(GetBookingsByHotelQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingsByHotelQueryHandler), request);

            try
            {
                // Check if hotel exists
                var hotelExists = await _dbContext.Hotels
                    .AsNoTracking()
                    .AnyAsync(h => h.Id == request.HotelId, cancellationToken);

                if (!hotelExists)
                {
                    Log.Warning("Hotel with ID {HotelId} does not exist.", request.HotelId);
                    throw new NotFoundException("Hotel", request.HotelId);
                }

                var skip = (request.Pagination.PageNumber - 1) * request.Pagination.PageSize;

                // Use compiled queries for repeated access pattern
                var totalCount = await BookingCompiledQueries.CountBookingsByHotelAsync(
                    _dbContext,
                    request.HotelId,
                    cancellationToken);

                var bookings = await BookingCompiledQueries.GetBookingsByHotelPageAsync(
                    _dbContext,
                    request.HotelId,
                    skip,
                    request.Pagination.PageSize,
                    cancellationToken);

                // Create paginated result
                var pagedList = new PagedList<BookingsForHotelDto>(
                    bookings,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("Retrieved {Count} bookings for hotel {HotelId} (page {PageNumber}) successfully",
                                totalCount, request.HotelId, request.Pagination.PageNumber);

                return ApiResponse<PagedList<BookingsForHotelDto>>.SuccessResponse(pagedList, "Hotel bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingsByHotelQueryHandler));
                throw;
            }
        }
    }
}
