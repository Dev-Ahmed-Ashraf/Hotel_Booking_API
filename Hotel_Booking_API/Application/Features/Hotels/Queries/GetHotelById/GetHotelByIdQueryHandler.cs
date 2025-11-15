using AutoMapper;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Exceptions;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotelById
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
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(
                request.Id,
                cancellationToken,
                h => h.Rooms
            );

            if (hotel == null)
                throw new NotFoundException("Hotel", request.Id);

            var hotelDto = _mapper.Map<HotelDto>(hotel);

            return ApiResponse<HotelDto>.SuccessResponse(hotelDto, "Hotel retrieved successfully.");
        }
    }
}
