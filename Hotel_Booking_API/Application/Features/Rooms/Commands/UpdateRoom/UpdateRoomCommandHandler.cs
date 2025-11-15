using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
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

        public async Task<ApiResponse<RoomDto>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {Handler} with request {@Request}", nameof(UpdateRoomCommandHandler), request);

            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                    request.Id, cancellationToken, r => r.Hotel);

                if (room == null || room.IsDeleted)
                    throw new NotFoundException("Room", request.Id);

                var dto = request.UpdateRoomDto;

                // Check duplicate room number
                if (!string.IsNullOrWhiteSpace(dto.RoomNumber) &&
                    !dto.RoomNumber.Equals(room.RoomNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicate = (await _unitOfWork.Rooms.FindAsync(r =>
                        r.HotelId == room.HotelId &&
                        r.RoomNumber.ToUpper() == dto.RoomNumber.ToUpper() &&
                        !r.IsDeleted &&
                        r.Id != room.Id)).FirstOrDefault();

                    if (duplicate != null)
                        throw new ConflictException($"A room with number '{dto.RoomNumber}' already exists in this hotel.");
                }

                // Apply partial updates
                if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
                    room.RoomNumber = dto.RoomNumber;

                if (dto.Type.HasValue)
                    room.Type = dto.Type.Value;

                if (dto.Price.HasValue)
                    room.Price = dto.Price.Value;

                if (dto.Capacity.HasValue)
                    room.Capacity = dto.Capacity.Value;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    room.Description = dto.Description;

                // Validate capacity compatibility with room type
                ValidateCapacityWithRoomType(room.Type, room.Capacity);

                room.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var roomDto = _mapper.Map<RoomDto>(room);
                roomDto.HotelName = room.Hotel.Name;

                Log.Information("Room updated successfully {RoomId}", room.Id);

                return ApiResponse<RoomDto>.SuccessResponse(roomDto, "Room updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing {Handler}", nameof(UpdateRoomCommandHandler));
                throw;
            }
        }

        /// <summary>
        /// Validates that the capacity is compatible with the room type.
        /// Throws ConflictException if the capacity exceeds the maximum allowed for the room type.
        /// </summary>
        /// <param name="roomType">The type of the room</param>
        /// <param name="capacity">The capacity to validate</param>
        private static void ValidateCapacityWithRoomType(RoomType roomType, int capacity)
        {
            var maxCapacity = roomType switch
            {
                RoomType.Standard => 2,
                RoomType.Deluxe => 3,
                RoomType.Suite => 4,
                RoomType.Presidential => 6,
                _ => 10
            };

            if (capacity > maxCapacity)
            {
                var roomTypeName = roomType.ToString();
                throw new ConflictException($"{roomTypeName} rooms can hold up to {maxCapacity} people only. " +
                    $"The provided capacity of {capacity} exceeds this limit.");
            }
        }
    }
}
