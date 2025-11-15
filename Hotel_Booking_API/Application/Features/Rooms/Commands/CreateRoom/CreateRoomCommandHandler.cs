using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
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

        public async Task<ApiResponse<RoomDto>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateRoomCommandHandler), request);

            try
            {
                // Validate hotel
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.CreateRoomDto.HotelId, cancellationToken);
                if (hotel == null || hotel.IsDeleted)
                    throw new NotFoundException(nameof(Hotel), request.CreateRoomDto.HotelId);

                // Validate room duplication
                var roomNumber = request.CreateRoomDto.RoomNumber.ToUpper();
                var duplicate = (await _unitOfWork.Rooms.FindAsync(r =>
                    r.HotelId == request.CreateRoomDto.HotelId &&
                    r.RoomNumber.ToUpper() == roomNumber &&
                    !r.IsDeleted)).FirstOrDefault();

                if (duplicate != null)
                    throw new ConflictException($"A room with number '{request.CreateRoomDto.RoomNumber}' already exists in this hotel.");

                // Create room
                var room = _mapper.Map<Room>(request.CreateRoomDto);
                room.CreatedAt = DateTime.UtcNow;
                room.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Rooms.AddAsync(room, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map DTO
                var dto = _mapper.Map<RoomDto>(room);
                dto.HotelName = hotel.Name;

                Log.Information("Room created successfully: Room {RoomId} in Hotel {HotelId}", room.Id, room.HotelId);

                return ApiResponse<RoomDto>.SuccessResponse(dto, "Room created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in {HandlerName}", nameof(CreateRoomCommandHandler));
                throw;
            }
        }
    }
}
