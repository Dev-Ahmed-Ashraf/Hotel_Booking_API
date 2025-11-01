using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using Humanizer;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Commands.CreateRoom
{
    /// <summary>
    /// Handler for creating a new room in the system.
    /// Validates business rules and creates the room if all conditions are met.
    /// </summary>
    public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, ApiResponse<RoomDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the room creation request by validating business rules and persisting the room.
        /// </summary>
        /// <param name="request">The create room command containing room details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the created room details or error message</returns>
        public async Task<ApiResponse<RoomDto>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateRoomCommandHandler), request);

            try
            {
                // Validate that the hotel exists and is not deleted
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.CreateRoomDto.HotelId, cancellationToken);
                if (hotel == null || hotel.IsDeleted)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.CreateRoomDto.HotelId);
                    throw new NotFoundException(nameof(Hotel), request.CreateRoomDto.HotelId);
                }

            // Check if a room with the same number already exists in this hotel (case-insensitive)
            var existingRoom = (await _unitOfWork.Rooms
                .FindAsync(r => r.HotelId == request.CreateRoomDto.HotelId && 
                               r.RoomNumber.ToLower() == request.CreateRoomDto.RoomNumber.ToLower() && 
                               !r.IsDeleted)).FirstOrDefault();

                if (existingRoom != null)
                {
                    Log.Warning("Duplicate room found: {RoomNumber} in hotel {HotelId}", request.CreateRoomDto.RoomNumber, request.CreateRoomDto.HotelId);
                    throw new ConflictException($"A room with number '{request.CreateRoomDto.RoomNumber}' already exists in this hotel.");
                }

            // Map DTO to entity and set default values
            var room = _mapper.Map<Room>(request.CreateRoomDto);
            room.CreatedAt = DateTime.UtcNow;
            room.UpdatedAt = DateTime.UtcNow;

            // Add room to repository and save changes
            await _unitOfWork.Rooms.AddAsync(room, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var roomDto = _mapper.Map<RoomDto>(room);
                roomDto.HotelName = hotel.Name; // Include hotel name in response

                Log.Information("Room created successfully with ID {RoomId} and number {RoomNumber} in hotel {HotelId}", room.Id, room.RoomNumber, room.HotelId);
                Log.Information("Completed {HandlerName} successfully", nameof(CreateRoomCommandHandler));

                return ApiResponse<RoomDto>.SuccessResponse(roomDto, "Room created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(CreateRoomCommandHandler));
                throw;
            }
        }
    }
}
