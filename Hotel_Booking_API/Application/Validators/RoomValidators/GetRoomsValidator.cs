using FluentValidation;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetRooms;

namespace Hotel_Booking_API.Application.Validators.RoomValidators
{
    /// <summary>
    /// Validator for GetRoomsQuery to ensure all search parameters are valid.
    /// Validates pagination, search criteria, and business rules.
    /// </summary>
    public class GetRoomsValidator : AbstractValidator<GetRoomsQuery>
    {
        public GetRoomsValidator()
        {
            // Validate pagination parameters
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Page number cannot exceed 1000");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            // Validate search criteria when provided
            When(x => x.Search != null, () =>
            {
                RuleFor(x => x.Search!.HotelId)
                    .GreaterThan(0).WithMessage("Hotel ID must be greater than 0")
                    .When(x => x.Search!.HotelId.HasValue);

                RuleFor(x => x.Search!.HotelName)
                    .MaximumLength(200).WithMessage("Hotel name cannot exceed 200 characters")
                    .Matches(@"^[A-Za-z0-9\s\-\.]+$").WithMessage("Hotel name can only contain letters, numbers, spaces, hyphens, and dots")
                    .When(x => !string.IsNullOrWhiteSpace(x.Search!.HotelName));

                RuleFor(x => x.Search!.RoomNumber)
                    .MaximumLength(50).WithMessage("Room number cannot exceed 50 characters")
                    .Matches(@"^[A-Za-z0-9\-\s]+$").WithMessage("Room number can only contain letters, numbers, hyphens, and spaces")
                    .When(x => !string.IsNullOrWhiteSpace(x.Search!.RoomNumber));

                RuleFor(x => x.Search!.Type)
                    .IsInEnum().WithMessage("Invalid room type")
                    .When(x => x.Search!.Type.HasValue);

                RuleFor(x => x.Search!.MinPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative")
                    .LessThanOrEqualTo(10000).WithMessage("Minimum price cannot exceed $10,000")
                    .When(x => x.Search!.MinPrice.HasValue);

                RuleFor(x => x.Search!.MaxPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative")
                    .LessThanOrEqualTo(10000).WithMessage("Maximum price cannot exceed $10,000")
                    .When(x => x.Search!.MaxPrice.HasValue);

                // Ensure max price is greater than min price when both are provided
                RuleFor(x => x.Search!.MaxPrice)
                    .GreaterThan(x => x.Search!.MinPrice).WithMessage("Maximum price must be greater than minimum price")
                    .When(x => x.Search!.MinPrice.HasValue && x.Search!.MaxPrice.HasValue);

                RuleFor(x => x.Search!.Capacity)
                    .GreaterThan(0).WithMessage("Capacity must be greater than 0")
                    .LessThanOrEqualTo(10).WithMessage("Capacity cannot exceed 10 guests")
                    .When(x => x.Search!.Capacity.HasValue);
            });
        }
    }
}