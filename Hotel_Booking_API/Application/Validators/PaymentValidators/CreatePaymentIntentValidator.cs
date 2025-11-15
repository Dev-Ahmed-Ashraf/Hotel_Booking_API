using FluentValidation;
using Hotel_Booking_API.Application.Features.Payments.Commands.CreatePaymentIntent;

namespace Hotel_Booking_API.Application.Validators.PaymentValidators
{
    public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentCommand>
    {
        public CreatePaymentIntentValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("BookingId must be greater than 0");
        }
    }
}


