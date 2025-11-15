using FluentValidation;
using Hotel_Booking_API.Application.Features.AdminDashboard.Queries;

namespace Hotel_Booking_API.Application.Validators.AdminDashboardValidators
{
    public class GetDashboardStatsValidator : AbstractValidator<GetDashboardStatsQuery>
    {
        public GetDashboardStatsValidator()
        {
            // No fields to validate; keep class for pipeline consistency and future args
            RuleFor(_ => 1).Equal(1);
        }
    }
}


