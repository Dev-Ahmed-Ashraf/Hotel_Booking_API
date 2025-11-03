using AutoMapper;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Interfaces;
using MediatR;

namespace Hotel_Booking_API.Application.Features.Payments.Queries
{
    public class GetPaymentByIdQuery : IRequest<PaymentDto>
    {
        public int Id { get; set; }
    }

    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPaymentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id, cancellationToken, p => p.Booking);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Payment {request.Id} not found");
            }
            return _mapper.Map<PaymentDto>(payment);
        }
    }
}


