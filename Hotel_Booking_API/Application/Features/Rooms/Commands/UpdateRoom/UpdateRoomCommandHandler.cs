using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.UpdateRoom
{
    /// <summary>
    /// Handler for updating an existing room in the system.
    /// Validates business rules and updates the room if all conditions are met.
    /// </summary>
    public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, ApiResponse<RoomDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the room update request by validating business rules and persisting changes.
        /// </summary>
        /// <param name="request">The update room command containing room ID and update details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the updated room details or error message</returns>
        public async Task<ApiResponse<RoomDto>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(UpdateRoomCommandHandler), request);

            try
            {
                // Get the existing room with hotel information
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                request.Id, 
                cancellationToken, 
                r => r.Hotel
            );

                if (room == null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.Id);
                    throw new NotFoundException("Room", request.Id);
                }

            var dto = request.UpdateRoomDto;

            // Check for duplicate room number if user wants to change it
            if (!string.IsNullOrWhiteSpace(dto.RoomNumber) && 
                !dto.RoomNumber.Equals(room.RoomNumber, StringComparison.OrdinalIgnoreCase))
            {
                var existingRoom = (await _unitOfWork.Rooms
                    .FindAsync(r => r.HotelId == room.HotelId && 
                                   r.RoomNumber.ToLower() == dto.RoomNumber.ToLower() && 
                                   !r.IsDeleted && 
                                   r.Id != room.Id)).FirstOrDefault();

                if (existingRoom != null)
                {
                    Log.Warning("Duplicate room number found: {RoomNumber} in hotel {HotelId}", dto.RoomNumber, room.HotelId);
                    throw new ConflictException($"A room with number '{dto.RoomNumber}' already exists in this hotel.");
                }
            }

            // Apply partial updates - only update fields that are provided (not null)
            if (!string.IsNullOrWhiteSpace(dto.RoomNumber)) 
                room.RoomNumber = dto.RoomNumber;

            if (dto.Type.HasValue)
                room.Type = dto.Type.Value;
            
            if (dto.Price > 0) 
                room.Price = dto.Price;
            
            if (dto.Capacity > 0) 
                room.Capacity = dto.Capacity;
            
            if (!string.IsNullOrWhiteSpace(dto.Description)) 
                room.Description = dto.Description;

            // Update availability if provided
            //if (dto.IsAvailable.HasValue && dto.IsAvailable.Value != room.IsAvailable)
            //    room.IsAvailable = dto.IsAvailable.Value;

            // Update timestamp
            room.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _unitOfWork.Rooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var roomDto = _mapper.Map<RoomDto>(room);
                roomDto.HotelName = room.Hotel.Name;

                Log.Information("Room updated successfully with ID {RoomId} and number {RoomNumber} in hotel {HotelId}", room.Id, room.RoomNumber, room.HotelId);
                Log.Information("Completed {HandlerName} successfully", nameof(UpdateRoomCommandHandler));

                return ApiResponse<RoomDto>.SuccessResponse(roomDto, "Room updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(UpdateRoomCommandHandler));
                throw;
            }
        }
    }
}
