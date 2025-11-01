using AutoMapper;
using Hotel_Booking.Application.Features.Hotels.Commands.UpdateHotel;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Hotels.Commands.UpdateHotel
{
    /// <summary>
    /// Handler for updating an existing hotel in the system.
    /// Validates business rules and updates the hotel if all conditions are met.
    /// </summary>
    public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, ApiResponse<HotelDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateHotelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the hotel update request by validating business rules and persisting changes.
        /// </summary>
        /// <param name="request">The update hotel command containing hotel ID and update details</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>ApiResponse containing the updated hotel details or error message</returns>
        public async Task<ApiResponse<HotelDto>> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(UpdateHotelCommandHandler), request);

            try
            {
                // Get the existing hotel
                var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.Id, cancellationToken);

                if (hotel == null || hotel.IsDeleted)
                {
                    Log.Warning("Hotel not found or deleted: {HotelId}", request.Id);
                    throw new NotFoundException("Hotel", request.Id);
                }

                var dto = request.UpdateHotelDto;

                // Check for duplicate name (only if user wants to change the name)
                if (!string.IsNullOrWhiteSpace(dto.Name) &&
                    !dto.Name.Equals(hotel.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var existingHotel = (await _unitOfWork.Hotels
                        .FindAsync(h => h.Name.ToLower() == dto.Name.ToLower() && !h.IsDeleted)).FirstOrDefault();

                    if (existingHotel != null && existingHotel.Id != hotel.Id)
                    {
                        Log.Warning("Hotel name already exists: {HotelName}", dto.Name);
                        throw new ConflictException($"A hotel with the name '{dto.Name}' already exists.");
                    }
                }

                // Apply partial updates - only update fields that are provided (not null)
                if (!string.IsNullOrWhiteSpace(dto.Name)) 
                    hotel.Name = dto.Name;
                
                if (!string.IsNullOrWhiteSpace(dto.Description)) 
                    hotel.Description = dto.Description;
                
                if (!string.IsNullOrWhiteSpace(dto.Address)) 
                    hotel.Address = dto.Address;
                
                if (!string.IsNullOrWhiteSpace(dto.City)) 
                    hotel.City = dto.City;
                
                if (!string.IsNullOrWhiteSpace(dto.Country)) 
                    hotel.Country = dto.Country;
                
                if (dto.Rating.HasValue) 
                    hotel.Rating = dto.Rating.Value;

                // Update timestamp
                hotel.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _unitOfWork.Hotels.UpdateAsync(hotel);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map entity back to DTO for response
                var hotelDto = _mapper.Map<HotelDto>(hotel);

                Log.Information("Hotel updated successfully with ID {HotelId} and name {HotelName}", hotel.Id, hotel.Name);
                Log.Information("Completed {HandlerName} successfully", nameof(UpdateHotelCommandHandler));

                return ApiResponse<HotelDto>.SuccessResponse(hotelDto, "Hotel updated successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(UpdateHotelCommandHandler));
                throw;
            }
        }
    }
}
