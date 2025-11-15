using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Rooms.Queries.GetRoomById
{
    /// <summary>
    /// Handler for retrieving a specific room by its ID.
    /// Includes hotel information and validates room existence.
    /// </summary>
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, ApiResponse<RoomDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<RoomDto>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetRoomByIdQueryHandler), request);

            try
            {
                // Get the room with hotel information included
                var room = await _unitOfWork.Rooms.GetByIdAsync(
                request.Id,
                cancellationToken,
                r => r.Hotel
                );

                // Check if room exists and is not deleted
                if (room == null || room.IsDeleted)
                {
                    Log.Warning("Room not found or deleted: {RoomId}", request.Id);
                    throw new NotFoundException("Room", request.Id);
                }

                // Map entity to DTO for response
                var roomDto = _mapper.Map<RoomDto>(room);

                Log.Information("Room retrieved successfully with ID {RoomId} and number {RoomNumber} in hotel {HotelId}", room.Id, room.RoomNumber, room.HotelId);

                return ApiResponse<RoomDto>.SuccessResponse(roomDto, "Room retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetRoomByIdQueryHandler));
                throw;
            }
        }
    }
}
