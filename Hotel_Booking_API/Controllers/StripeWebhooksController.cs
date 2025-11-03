using Hotel_Booking_API.Application.Events;
using Hotel_Booking_API.Domain.Entities;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Stripe;

namespace Hotel_Booking_API.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhooksController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public StripeWebhooksController(IStripeService stripeService, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _stripeService = stripeService;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                Log.Warning("Stripe webhook received without signature header");
                return BadRequest();
            }

            Event stripeEvent;
            try
            {
                stripeEvent = _stripeService.VerifyWebhookSignature(json, signatureHeader);
            }
            catch
            {
                return Unauthorized();
            }

            Log.Information("Stripe webhook: {Type} {Id}", stripeEvent.Type, stripeEvent.Id);

            if (stripeEvent.Type == Events.PaymentIntentSucceeded || stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                var paymentIntent = (stripeEvent.Data.Object as PaymentIntent)!;
                var piId = paymentIntent.Id;

                // Find local payment by TransactionId (which stores the PaymentIntent Id)
                var payments = await _unitOfWork.Payments.FindAsync(p => p.TransactionId == piId);
                var payment = payments.FirstOrDefault();
                if (payment == null)
                {
                    Log.Warning("Webhook for unknown PaymentIntent: {PaymentIntentId}", piId);
                    return Ok();
                }

                // Idempotency: if already terminal, ignore
                if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Failed || payment.Status == PaymentStatus.Refunded || payment.Status == PaymentStatus.Cancelled)
                {
                    return Ok();
                }

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;

                    var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId, default);
                    if (booking != null && booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Confirmed;
                        await _unitOfWork.Bookings.UpdateAsync(booking);
                    }

                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();

                    // Publish event for email notification
                    try
                    {
                        var paymentSucceededEvent = new PaymentSucceededEvent
                        {
                            PaymentId = payment.Id,
                            BookingId = payment.BookingId,
                            UserId = booking?.UserId ?? 0,
                            Amount = payment.Amount,
                            TransactionId = payment.TransactionId
                        };

                        await _mediator.Publish(paymentSucceededEvent);
                        Log.Information("Published PaymentSucceededEvent for PaymentId: {PaymentId}", payment.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error publishing PaymentSucceededEvent for PaymentId: {PaymentId}", payment.Id);
                        // Don't fail the webhook if email fails
                    }
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    payment.Status = PaymentStatus.Failed;
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return Ok();
        }
    }
}


