using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookings
{
    /// <summary>
    /// Handler for retrieving a paginated list of bookings with optional filtering.
    /// Implements complex filtering logic and returns bookings with related information.
    /// </summary>
    public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, ApiResponse<PagedList<BookingDto>>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetBookingsQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedList<BookingDto>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingsQueryHandler), request);

            try
            {
                // Start with base query including related entities
                IQueryable<Booking> query = _context.Bookings
                    .IgnoreQueryFilters()
                    .Include(b => b.User)
                    .Include(b => b.Room).ThenInclude(r => r.Hotel)
                    .Include(b => b.Payment)
                    .AsNoTracking();

                // Soft delete
                query = request.IncludeDeleted ? query : query.Where(b => !b.IsDeleted);

                // Filters
                if (request.Search is { } search)
                {
                    if (search.HotelId is int hotelId)
                        query = query.Where(b => b.Room!.HotelId == hotelId);

                    if (search.UserId is int userId)
                        query = query.Where(b => b.UserId == userId);

                    if (search.Status is BookingStatus status)
                        query = query.Where(b => b.Status == status);

                    if (search.StartDate is DateTime start)
                        query = query.Where(b => b.CheckInDate >= start);

                    if (search.EndDate is DateTime end)
                        query = query.Where(b => b.CheckOutDate <= end);
                }

                // Count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination and ordering
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

                Log.Information("Bookings retrieved successfully: {TotalCount} bookings found for page {PageNumber}",
                totalCount, request.Pagination.PageNumber);

                return ApiResponse<PagedList<BookingDto>>.SuccessResponse(pagedList, "Bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingsQueryHandler));
                throw;
            }
        }
    }
}
