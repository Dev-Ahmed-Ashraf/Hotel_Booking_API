using FluentValidation;
using Hotel_Booking_API.Application.Features.Hotels.Commands.UpdateHotel;

namespace Hotel_Booking_API.Application.Validators.HotelValidators
{
    /// <summary>
    /// Validator for UpdateHotelCommand to ensure all business rules and constraints are met.
    /// Validates hotel data before update to maintain data integrity.
    /// </summary>
    public class UpdateHotelValidator : AbstractValidator<UpdateHotelCommand>
    {
        public UpdateHotelValidator()
        {
            // Validate hotel ID
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Hotel ID must be greater than 0");

            // Validate hotel name if provided
            RuleFor(x => x.UpdateHotelDto.Name)
                .NotEmpty().WithMessage("Hotel name cannot be empty")
                .MinimumLength(3).WithMessage("Hotel name must be at least 3 characters")
                .MaximumLength(200).WithMessage("Hotel name cannot exceed 200 characters")
                .Matches(@"^[A-Za-z0-9\s\-\.&'()]+$").WithMessage("Hotel name can only contain letters, numbers, spaces, hyphens, dots, ampersands, and parentheses")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateHotelDto.Name));

            // Validate description if provided
            RuleFor(x => x.UpdateHotelDto.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateHotelDto.Description));

            // Validate address if provided
            RuleFor(x => x.UpdateHotelDto.Address)
                .NotEmpty().WithMessage("Address cannot be empty")
                .MinimumLength(10).WithMessage("Address must be at least 10 characters")
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
                .Matches(@"^[A-Za-z0-9\s\-\.#,]+$").WithMessage("Address can only contain letters, numbers, spaces, hyphens, dots, commas, and hash symbols")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateHotelDto.Address));

            // Validate city if provided
            RuleFor(x => x.UpdateHotelDto.City)
                .NotEmpty().WithMessage("City name cannot be empty")
                .MinimumLength(2).WithMessage("City name must be at least 2 characters")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("City name can only contain letters, spaces, hyphens, dots, and apostrophes")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateHotelDto.City));

            // Validate country if provided
            RuleFor(x => x.UpdateHotelDto.Country)
                .NotEmpty().WithMessage("Country name cannot be empty")
                .MinimumLength(2).WithMessage("Country name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
                .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("Country name can only contain letters, spaces, hyphens, dots, and apostrophes")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateHotelDto.Country));

            // Validate rating if provided
            RuleFor(x => x.UpdateHotelDto.Rating)
                .GreaterThanOrEqualTo(0).WithMessage("Rating must be at least 0")
                .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5")
                .When(x => x.UpdateHotelDto.Rating.HasValue);
        }
    }
}
