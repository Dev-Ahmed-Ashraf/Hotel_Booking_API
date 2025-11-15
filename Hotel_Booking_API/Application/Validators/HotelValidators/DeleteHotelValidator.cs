using FluentValidation;
using Hotel_Booking_API.Application.Features.Hotels.Commands.DeleteHotel;

namespace Hotel_Booking_API.Application.Validators.HotelValidators
{
    public class DeleteHotelValidator : AbstractValidator<DeleteHotelCommand>
    {
        public DeleteHotelValidator()
        {
            RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Hotel Id must be greater than zero.");
        }
    }
}
