using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
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

        /// <summary>
        /// Handles the get bookings query by applying filters and pagination.
        /// All validation is handled by the GetBookingsValidator through the MediatR pipeline.
        /// </summary>
        /// <param name="request">The query containing pagination and search parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing paginated list of bookings or error message</returns>
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
                    .AsNoTracking()
                    .AsQueryable();

                // Filter out soft-deleted bookings unless explicitly requested
                if (!request.IncludeDeleted)
                    query = query.Where(b => !b.IsDeleted);

                // Apply search filters if provided
                if (request.Search is not null)
                {
                    var search = request.Search;

                    // Filter by hotel ID
                    if (search.HotelId.HasValue)
                        query = query.Where(b => b.Room!.HotelId == search.HotelId.Value);

                    // Filter by user ID
                    if (search.UserId.HasValue)
                        query = query.Where(b => b.UserId == search.UserId.Value);

                    // Filter by status
                    if (search.Status.HasValue)
                        query = query.Where(b => b.Status == search.Status.Value);

                    // Filter by date range
                    if (search.StartDate.HasValue)
                        query = query.Where(b => b.CheckInDate >= search.StartDate.Value);

                    if (search.EndDate.HasValue)
                        query = query.Where(b => b.CheckOutDate <= search.EndDate.Value);
                }

                // Get total count for pagination
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
