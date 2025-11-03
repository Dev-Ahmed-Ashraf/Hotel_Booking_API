using AutoMapper;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Payments.Queries
{
    public class GetPaymentByBookingQuery : IRequest<PaymentDto>
    {
        public int BookingId { get; set; }
    }

    public class GetPaymentByBookingQueryHandler : IRequestHandler<GetPaymentByBookingQuery, PaymentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaymentByBookingQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto> Handle(GetPaymentByBookingQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId, cancellationToken, b => b.Payment);
            if (bookings == null || bookings.Payment == null)
            {
                throw new KeyNotFoundException($"Payment for booking {request.BookingId} not found");
            }
            return _mapper.Map<PaymentDto>(bookings.Payment);
        }
    }
}


