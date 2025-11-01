using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Hotels.Commands.DeleteHotel
{
    /// <summary>
    /// Handler for deleting a hotel from the system.
    /// Validates business rules and performs soft or hard delete based on request.
    /// </summary>
    public class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand, ApiResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteHotelCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles the hotel deletion request by validating business rules and removing the hotel.
        /// </summary>
        /// <param name="request">The delete hotel command containing hotel ID and deletion options</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing success message or error message</returns>
        public async Task<ApiResponse<string>> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(DeleteHotelCommandHandler), request);

            try
            {
                // Get the hotel with related rooms and bookings to check for active reservations
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(
                    request.Id, 
                    cancellationToken, 
                    h => h.Rooms
                );

                if (hotel == null)
                {
                    Log.Warning("Hotel not found: {HotelId}", request.Id);
                    throw new NotFoundException("Hotel", request.Id);
                }

                if (hotel.IsDeleted)
                {
                    Log.Warning("Hotel already deleted: {HotelId}", request.Id);
                    throw new BadRequestException($"Hotel with ID {request.Id} is already deleted.");
                }

                // Check for active bookings if not forcing deletion
                if (!request.ForceDelete)
                {
                    // Get all rooms for this hotel and check for active bookings
                    var rooms = await _unitOfWork.Rooms.FindAsync(r => r.HotelId == hotel.Id && !r.IsDeleted);
                    
                    foreach (var room in rooms)
                    {
                        var activeBookings = await _unitOfWork.Bookings.FindAsync(b => 
                            b.RoomId == room.Id && 
                            !b.IsDeleted && 
                            b.Status != Domain.Enums.BookingStatus.Cancelled &&
                            b.CheckOutDate > DateTime.UtcNow
                        );

                        if (activeBookings.Any())
                        {
                            var bookingIds = string.Join(", ", activeBookings.Select(b => b.Id));
                            Log.Warning("Cannot delete hotel with active bookings: {HotelId}, BookingIds: {BookingIds}", request.Id, bookingIds);
                            throw new BadRequestException($"Cannot delete hotel '{hotel.Name}' because it has active bookings (IDs: {bookingIds}). Use ForceDelete=true to override this check, but this may cause data integrity issues.");
                        }
                    }
                }

                // Perform deletion based on request
                if (request.IsSoft)
                {
                    // Soft delete: mark as deleted but keep the record
                    hotel.IsDeleted = true;
                    hotel.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Hotels.UpdateAsync(hotel);
                    Log.Information("Hotel soft deleted successfully with ID {HotelId} and name {HotelName}", hotel.Id, hotel.Name);
                }
                else
                {
                    // Hard delete: permanently remove the record
                    await _unitOfWork.Hotels.DeleteAsync(hotel);
                    Log.Information("Hotel permanently deleted with ID {HotelId} and name {HotelName}", hotel.Id, hotel.Name);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var message = request.IsSoft
                    ? $"Hotel '{hotel.Name}' soft deleted successfully."
                    : $"Hotel '{hotel.Name}' permanently deleted.";

                Log.Information("Completed {HandlerName} successfully", nameof(DeleteHotelCommandHandler));

                return ApiResponse<string>.SuccessResponse(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(DeleteHotelCommandHandler));
                throw;
            }
        }
    }
}
