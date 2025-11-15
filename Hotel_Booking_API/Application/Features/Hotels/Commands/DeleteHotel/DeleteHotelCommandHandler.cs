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

        public async Task<ApiResponse<string>> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {Handler} with request {@Request}", nameof(DeleteHotelCommandHandler), request);

            try
            {
                // Load basic hotel data
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.Id, cancellationToken);

                if (hotel == null)
                    throw new NotFoundException("Hotel", request.Id);

                if (hotel.IsDeleted)
                    throw new BadRequestException($"Hotel with ID {request.Id} is already deleted.");

                // Check active bookings (if not forcing delete)
                if (!request.ForceDelete)
                {
                    // Query all active bookings for this hotel (1 query only)
                    var activeBookings = await _unitOfWork.Bookings
                        .FindAsync(b =>
                            b.Room!.HotelId == hotel.Id &&
                            !b.IsDeleted &&
                            b.Status != Domain.Enums.BookingStatus.Cancelled &&
                            b.CheckOutDate > DateTime.UtcNow);

                    if (activeBookings.Any())
                    {
                        var ids = string.Join(", ", activeBookings.Select(b => b.Id));
                        throw new BadRequestException(
                            $"Cannot delete hotel '{hotel.Name}' because it has active bookings (IDs: {ids}). " +
                            $"Use ForceDelete=true to override.");
                    }
                }

                // Apply deletion 
                if (request.IsSoft)
                {
                    hotel.IsDeleted = true;
                    hotel.UpdatedAt = DateTime.UtcNow;

                    // Soft delete all rooms belonging to this hotel (to keep consistency)
                    var rooms = await _unitOfWork.Rooms.FindAsync(r => r.HotelId == hotel.Id);

                    foreach (var room in rooms)
                    {
                        room.IsDeleted = true;
                        room.UpdatedAt = DateTime.UtcNow;
                    }

                    Log.Information("?? Soft-deleted hotel {HotelId} and all its rooms ({RoomsCount})",
                        hotel.Id, rooms.Count());
                }
                else
                {
                    await _unitOfWork.Hotels.DeleteAsync(hotel);
                    Log.Information("Hard deleted hotel {HotelId}", hotel.Id);
                }

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var msg = request.IsSoft
                    ? $"Hotel '{hotel.Name}' soft deleted successfully."
                    : $"Hotel '{hotel.Name}' permanently deleted.";

                return ApiResponse<string>.SuccessResponse(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {Handler}", nameof(DeleteHotelCommandHandler));
                throw;
            }
        }
    }
}
