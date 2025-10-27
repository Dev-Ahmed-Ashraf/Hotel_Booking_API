using MediatR;
using AutoMapper;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Hotels.Commands.CreateHotel
{
    /// <summary>
    /// Handler for creating a new hotel in the system.
    /// Validates business rules and creates the hotel if all conditions are met.
    /// </summary>
    public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, ApiResponse<HotelDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateHotelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the hotel creation request by validating business rules and persisting the hotel.
        /// </summary>
        /// <param name="request">The create hotel command containing hotel details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the created hotel details or error message</returns>
        public async Task<ApiResponse<HotelDto>> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(CreateHotelCommandHandler), request);

            try
            {
                // Check if a hotel with the same name already exists (case-insensitive)
                var existingHotel = (await _unitOfWork.Hotels
                    .FindAsync(h => h.Name.ToLower() == request.CreateHotelDto.Name.ToLower() && !h.IsDeleted)).FirstOrDefault();

                if (existingHotel != null)
                {
                    Log.Warning("Hotel name already exists: {HotelName}", request.CreateHotelDto.Name);
                    return ApiResponse<HotelDto>.ErrorResponse($"A hotel with the name '{request.CreateHotelDto.Name}' already exists.");
                }

                // Map DTO to entity and set default values
                var hotel = _mapper.Map<Hotel>(request.CreateHotelDto);
                hotel.CreatedAt = DateTime.UtcNow;
                hotel.UpdatedAt = DateTime.UtcNow;
                hotel.IsDeleted = false; // Ensure new hotels are not deleted

                // Add hotel to repository and save changes
                await _unitOfWork.Hotels.AddAsync(hotel, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var hotelDto = _mapper.Map<HotelDto>(hotel);
                hotelDto.TotalRooms = 0; // New hotels start with 0 rooms
                hotelDto.AvailableRooms = 0;

                Log.Information("Hotel created successfully with ID {HotelId} and name {HotelName}", hotel.Id, hotel.Name);
                Log.Information("Completed {HandlerName} successfully", nameof(CreateHotelCommandHandler));

                return ApiResponse<HotelDto>.SuccessResponse(hotelDto, "Hotel created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(CreateHotelCommandHandler));
                throw;
            }
        }
    }
}
