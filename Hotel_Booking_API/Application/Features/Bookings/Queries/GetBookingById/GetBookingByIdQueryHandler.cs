using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingById
{
    /// <summary>
    /// Handler for retrieving a specific booking by its ID.
    /// Includes full booking details with related entities.
    /// </summary>
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, ApiResponse<BookingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetBookingByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetBookingByIdQueryHandler), request);

            try
            {
                // Get the booking with all related entities included
                var booking = await _unitOfWork.Bookings.GetByIdAsync(
                    request.Id,
                    cancellationToken,
                    b => b.User,
                    b => b.Room,
                    b => b.Room.Hotel,
                    b => b.Payment
                );

                // Check if booking exists and is not deleted
                if (booking is null || booking.IsDeleted)
                {
                    Log.Warning("Booking not found or deleted: {BookingId}", request.Id);
                    throw new NotFoundException("Booking", request.Id);
                }

                // Map entity to DTO for response
                var bookingDto = _mapper.Map<BookingDto>(booking);

                Log.Information("Booking retrieved successfully with ID {BookingId} for user {UserId}", booking.Id, booking.UserId);

                return ApiResponse<BookingDto>.SuccessResponse(bookingDto, "Booking retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetBookingByIdQueryHandler));
                throw;
            }
        }
    }
}
