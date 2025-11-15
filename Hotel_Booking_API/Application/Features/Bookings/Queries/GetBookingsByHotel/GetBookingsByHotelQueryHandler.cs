using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByHotel
{
    public class GetBookingsByHotelQueryHandler : IRequestHandler<GetBookingsByHotelQuery, ApiResponse<PagedList<BookingDto>>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetBookingsByHotelQueryHandler(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedList<BookingDto>>> Handle(GetBookingsByHotelQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingsByHotelQueryHandler), request);

            try
            {
                // Start with base query for hotel bookings
                IQueryable<Booking> query = _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Room.HotelId == request.HotelId && !b.IsDeleted);

                // Get total count for pagination
                var totalCount = await query.CountAsync(cancellationToken);

                if (totalCount == 0)
                {
                    Log.Warning("Hotel {HotelId} has no active bookings.", request.HotelId);
                    throw new NotFoundException($"No active bookings found for hotel with ID {request.HotelId}.");
                }


                var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ProjectTo<BookingDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

                // Create paginated result
                var pagedList = new PagedList<BookingDto>(
                    bookings,
                    request.Pagination.PageNumber,
                    request.Pagination.PageSize,
                    totalCount
                );

                Log.Information("Retrieved {Count} bookings for hotel {HotelId} (page {PageNumber}) successfully",
                                totalCount, request.HotelId, request.Pagination.PageNumber);


                return ApiResponse<PagedList<BookingDto>>.SuccessResponse(pagedList, "Hotel bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingsByHotelQueryHandler));
                throw;
            }
        }
    }
}
