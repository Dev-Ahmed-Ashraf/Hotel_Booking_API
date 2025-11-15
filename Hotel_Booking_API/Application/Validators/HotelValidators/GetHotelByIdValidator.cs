using FluentValidation;
using Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotelById;

namespace Hotel_Booking_API.Application.Validators.HotelValidators
{
    public class GetHotelByIdValidator : AbstractValidator<GetHotelByIdQuery>
    {
        public GetHotelByIdValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Hotel ID must be greater than zero.");
        }
    }
}
