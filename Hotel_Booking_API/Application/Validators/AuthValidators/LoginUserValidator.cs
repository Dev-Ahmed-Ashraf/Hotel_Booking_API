using FluentValidation;
using Hotel_Booking_API.Application.DTOs;

namespace Hotel_Booking.Application.Validators.AuthValidators
{
    public class LoginUserValidator : AbstractValidator<LoginDto>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
