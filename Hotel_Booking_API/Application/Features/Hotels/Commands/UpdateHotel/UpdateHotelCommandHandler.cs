using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Hotels.Commands.UpdateHotel;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

/// <summary>
/// Handles updating an existing hotel by applying partial updates
/// and validating business rules.
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

    public async Task<ApiResponse<HotelDto>> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Starting {Handler} with request {@Request}", nameof(UpdateHotelCommandHandler), request);

        try
        {
            // Retrieve hotel (tracked entity)
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.Id, cancellationToken);

            if (hotel == null || hotel.IsDeleted)
            {
                Log.Warning("Hotel not found or deleted: {HotelId}", request.Id);
                throw new NotFoundException("Hotel", request.Id);
            }

            var dto = request.UpdateHotelDto;

            // Validate new name if changed
            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                !dto.Name.Equals(hotel.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = (await _unitOfWork.Hotels
                    .FindAsync(h => h.Name.ToLower() == dto.Name.ToLower() && !h.IsDeleted))
                    .Any();

                if (exists)
                {
                    Log.Warning("Hotel name already exists: {HotelName}", dto.Name);
                    throw new ConflictException($"A hotel with the name '{dto.Name}' already exists.");
                }
            }

            // Apply partial updates
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

            // Save changes (EF tracks the entity automatically)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Map and return response
            var responseDto = _mapper.Map<HotelDto>(hotel);

            Log.Information("Hotel updated successfully with ID {HotelId}", hotel.Id);

            return ApiResponse<HotelDto>.SuccessResponse(responseDto, "Hotel updated successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while processing {Handler}", nameof(UpdateHotelCommandHandler));
            throw;
        }
    }
}
