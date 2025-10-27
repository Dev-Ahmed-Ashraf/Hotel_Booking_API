using FluentValidation;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Hotels.Commands.CreateHotel;

namespace Hotel_Booking_API.Application.Validators.HotelValidators
{
    /// <summary>
    /// Validator for CreateHotelCommand to ensure all business rules and constraints are met.
    /// Validates hotel data before creation to maintain data integrity.
    /// </summary>
    public class CreateHotelValidator : AbstractValidator<CreateHotelCommand>
    {
        public CreateHotelValidator()
        {
            // Validate hotel name
            RuleFor(x => x.CreateHotelDto.Name)
                .NotEmpty().WithMessage("Hotel name is required")
                .MinimumLength(3).WithMessage("Hotel name must be at least 3 characters")
                .MaximumLength(200).WithMessage("Hotel name cannot exceed 200 characters")
                .Matches(@"^[A-Za-z0-9\s\-\.&'()]+$").WithMessage("Hotel name can only contain letters, numbers, spaces, hyphens, dots, ampersands, and parentheses");

            // Validate description
            RuleFor(x => x.CreateHotelDto.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.CreateHotelDto.Description));

            // Validate address
            RuleFor(x => x.CreateHotelDto.Address)
                .NotEmpty().WithMessage("Address is required")
                .MinimumLength(10).WithMessage("Address must be at least 10 characters")
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters")
                .Matches(@"^[A-Za-z0-9\s\-\.#,]+$").WithMessage("Address can only contain letters, numbers, spaces, hyphens, dots, commas, and hash symbols");

            // Validate city
            RuleFor(x => x.CreateHotelDto.City)
                .NotEmpty().WithMessage("City is required")
                .MinimumLength(2).WithMessage("City name must be at least 2 characters")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
                .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("City name can only contain letters, spaces, hyphens, dots, and apostrophes");

            // Validate country
            RuleFor(x => x.CreateHotelDto.Country)
                .NotEmpty().WithMessage("Country is required")
                .MinimumLength(2).WithMessage("Country name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
                .Matches(@"^[A-Za-z\s\-\.']+$").WithMessage("Country name can only contain letters, spaces, hyphens, dots, and apostrophes");

            // Validate rating
            RuleFor(x => x.CreateHotelDto.Rating)
                .GreaterThanOrEqualTo(0).WithMessage("Rating must be at least 0")
                .LessThanOrEqualTo(5).WithMessage("Rating cannot exceed 5");
        }
    }
}
