using FluentValidation;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByHotel;

namespace Hotel_Booking_API.Application.Validators.BookingValidators
{
    public class GetBookingsByHotelValidator : AbstractValidator<GetBookingsByHotelQuery>
    {
        public GetBookingsByHotelValidator()
        {
            // Validate Hotel ID
            RuleFor(x => x.HotelId)
                .GreaterThan(0).WithMessage("Hotel ID must be greater than 0");

            // Validate pagination parameters
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Page number cannot exceed 1000");

            // Validate pagination parameters
            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
