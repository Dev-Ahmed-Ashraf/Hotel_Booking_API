﻿using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Serilog;

namespace Hotel_Booking.Application.Features.Hotels.Queries.GetHotelById
{
    public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, ApiResponse<HotelDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetHotelByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<HotelDto>> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
        {
            Log.Information("Starting {HandlerName} with request {@Request}", nameof(GetHotelByIdQueryHandler), request);

            try
            {
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

                var hotelDto = _mapper.Map<HotelDto>(hotel);
                hotelDto.TotalRooms = hotel.Rooms.Count;
                //hotelDto.AvailableRooms = hotel.Rooms.Count(r => r.IsAvailable);

                Log.Information("Hotel retrieved successfully with ID {HotelId} and name {HotelName}", hotel.Id, hotel.Name);
                Log.Information("Completed {HandlerName} successfully", nameof(GetHotelByIdQueryHandler));

                return ApiResponse<HotelDto>.SuccessResponse(hotelDto, "Hotel retrieved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing {HandlerName}", nameof(GetHotelByIdQueryHandler));
                throw;
            }
        }
    }
}
