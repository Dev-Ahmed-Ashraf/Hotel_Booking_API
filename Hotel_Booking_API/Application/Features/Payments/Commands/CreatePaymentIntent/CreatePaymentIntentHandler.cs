using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Hotel_Booking_API.Application.Features.Payments.Commands.CreatePaymentIntent
{
    public class CreatePaymentIntentHandler : IRequestHandler<CreatePaymentIntentCommand, CreatePaymentIntentResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeService _stripeService;
        private readonly IConfiguration _configuration;

        public CreatePaymentIntentHandler(IUnitOfWork unitOfWork, IStripeService stripeService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _stripeService = stripeService;
            _configuration = configuration;
        }

        public async Task<CreatePaymentIntentResponseDto> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId, cancellationToken, b => b.Payment);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking {request.BookingId} not found");
            }

            if (booking.TotalPrice <= 0)
            {
                throw new InvalidOperationException("Booking total price must be greater than zero");
            }

            if (booking.Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Booking must be in Pending status to create payment");
            }

            if (booking.Payment != null && booking.Payment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Booking already paid");
            }

            var currency = _configuration.GetValue<string>("Stripe:Currency") ?? "usd";
            var description = $"Booking #{booking.Id} payment";
            var idempotencyKey = $"booking-{booking.Id}-v1";

            var (paymentIntentId, clientSecret) = await _stripeService.CreatePaymentIntentAsync(
                booking.TotalPrice,
                currency,
                description,
                idempotencyKey,
                cancellationToken);

            if (booking.Payment == null)
            {
                booking.Payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = PaymentMethod.CreditCard,
                    TransactionId = paymentIntentId
                };
                await _unitOfWork.Payments.AddAsync(booking.Payment, cancellationToken);
            }
            else
            {
                booking.Payment.Status = PaymentStatus.Pending;
                booking.Payment.TransactionId = paymentIntentId;
                await _unitOfWork.Payments.UpdateAsync(booking.Payment);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            Log.Information("PaymentIntent linked to booking {BookingId}: {PaymentIntentId}", booking.Id, paymentIntentId);

            return new CreatePaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntentId,
                ClientSecret = clientSecret
            };
        }
    }
}


