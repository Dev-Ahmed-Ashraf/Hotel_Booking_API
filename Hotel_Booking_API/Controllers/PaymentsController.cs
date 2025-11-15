using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Payments.Commands.CreatePaymentIntent;
using Hotel_Booking_API.Application.Features.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a Stripe PaymentIntent for a booking.
        /// </summary>
        [HttpPost("intents")]
        [ProducesResponseType(typeof(ApiResponse<CreatePaymentIntentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CreatePaymentIntentResponseDto>>> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentCommand command)
        {
            var dto = await _mediator.Send(command);

            return Ok(ApiResponse<CreatePaymentIntentResponseDto>.SuccessResponse(dto, "PaymentIntent created successfully"));
        }

        /// <summary>
        /// Gets a payment by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById([Range(1, int.MaxValue)] int id)
        {
            var dto = await _mediator.Send(new GetPaymentByIdQuery { Id = id });

            return Ok(ApiResponse<PaymentDto>.SuccessResponse(dto));
        }

        /// <summary>
        /// Gets the payment linked to a booking.
        /// </summary>
        [HttpGet("~/api/bookings/{bookingId:int}/payment")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentByBooking(
            [Range(1, int.MaxValue)] int bookingId)
        {
            var dto = await _mediator.Send(new GetPaymentByBookingQuery { BookingId = bookingId });

            return Ok(ApiResponse<PaymentDto>.SuccessResponse(dto));
        }
    }
}


