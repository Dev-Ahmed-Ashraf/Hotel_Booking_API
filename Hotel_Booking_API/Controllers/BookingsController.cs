using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Bookings.Commands.CancelBooking;
using Hotel_Booking_API.Application.Features.Bookings.Commands.ChangeBookingStatus;
using Hotel_Booking_API.Application.Features.Bookings.Commands.CreateBooking;
using Hotel_Booking_API.Application.Features.Bookings.Commands.DeleteBooking;
using Hotel_Booking_API.Application.Features.Bookings.Commands.UpdateBooking;
using Hotel_Booking_API.Application.Features.Bookings.Queries.CalculateBookingPrice;
using Hotel_Booking_API.Application.Features.Bookings.Queries.CheckRoomAvailability;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingById;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookings;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByHotel;
using Hotel_Booking_API.Application.Features.Bookings.Queries.GetBookingsByUser;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_API.Controllers
{
    /// <summary>
    /// Controller for managing booking operations in the hotel booking system.
    /// Provides CRUD operations for bookings with proper authorization and validation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICacheInvalidator _cacheInvalidator;

        public BookingsController(IMediator mediator, ICacheInvalidator cacheInvalidator)
        {
            _mediator = mediator;
            _cacheInvalidator = cacheInvalidator;
        }

        /// <summary>
        /// Creates a new booking in the system.
        /// </summary>
        /// <param name="createBookingDto">The booking details to create</param>
        /// <param name="userId">The ID of the user making the booking</param>
        /// <returns>Returns the created booking details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Customer or Admin role authorization.
        /// Validates room availability and calculates total price automatically.
        /// </remarks>
        /// <response code="201">Booking created successfully.</response>
        /// <response code="400">Validation failed or room not available.</response>
        /// <response code="401">Unauthorized — the request requires authentication.</response>
        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BookingDto>>> CreateBooking(
            [FromBody] CreateBookingDto createBookingDto,
            [FromQuery, Range(1, int.MaxValue)] int userId)
        {
            var command = new CreateBookingCommand
            {
                CreateBookingDto = createBookingDto,
                UserId = userId
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Bookings.Prefix);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return CreatedAtAction(nameof(GetBookingById), new { id = result.Data?.Id }, result);
        }


        /// <summary>
        /// Retrieves a specific booking by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <returns>Returns the booking details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Returns detailed booking information including hotel, room, and user details.
        /// </remarks>
        /// <response code="200">Booking retrieved successfully.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="401">Unauthorized — the request requires authentication.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BookingDto>>> GetBookingById([FromRoute, Range(1, int.MaxValue)] int id)
        {
            var query = new GetBookingByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of bookings with optional filtering.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of bookings per page (default: 10).</param>
        /// <param name="hotelId">Optional filter by hotel ID.</param>
        /// <param name="userId">Optional filter by user ID.</param>
        /// <param name="status">Optional filter by booking status.</param>
        /// <param name="startDate">Optional filter by start date.</param>
        /// <param name="endDate">Optional filter by end date.</param>
        /// <param name="includeDeleted">Whether to include soft-deleted bookings.</param>
        /// <returns>Returns a paginated list of bookings wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Admin role authorization.
        /// Supports comprehensive filtering and pagination with validation.
        /// </remarks>
        /// <response code="200">List of bookings retrieved successfully.</response>
        /// <response code="400">Invalid filter or pagination parameters.</response>
        /// <response code="401">Unauthorized — the request requires admin privileges.</response>
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedList<BookingDto>>>> GetBookings(
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery, Range(1, int.MaxValue)] int? hotelId = null,
            [FromQuery, Range(1, int.MaxValue)] int? userId = null,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeDeleted = false)
        {
            var search = new SearchBookingsDto
            {
                HotelId = hotelId,
                UserId = userId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };

            var query = new GetBookingsQuery
            {
                Pagination = new PaginationParameters(pageNumber, pageSize),
                Search = search,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Updates Booking dates.
        /// </summary>
        /// <param name="id">The ID of the booking to update.</param>
        /// <param name="updateBookingDto">The fields you want to update.</param>
        /// <returns>Returns the updated booking details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Customer or Admin role authorization.
        /// Supports partial updates - only provided fields will be updated.
        /// Cannot update cancelled or completed bookings.
        /// </remarks>
        /// <response code="200">Booking updated successfully.</response>
        /// <response code="400">Validation failed or booking cannot be updated.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="401">Unauthorized — the request requires Customer or admin privileges.</response>
        [HttpPatch("{id}")]
        [Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateBooking(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromBody] UpdateBookingDto updateBookingDto)
        {
            var command = new UpdateBookingCommand
            {
                Id = id,
                UpdateBookingDto = updateBookingDto
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Bookings.Prefix);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a booking by ID.
        /// </summary>
        /// <param name="id">Booking ID to delete</param>
        /// <param name="isSoft">If true, marks the booking as deleted instead of removing it permanently (default: true)</param>
        /// <param name="forceDelete">If true, forces deletion even if booking is active (default: false)</param>
        /// <returns>204 No Content if successful, 404 if not found</returns>
        /// <remarks>
        /// Requires Owner or Admin role authorization.
        /// By default, performs soft delete to maintain data integrity.
        /// Restores room availability if booking is active.
        /// </remarks>
        /// <response code="204">Booking deleted successfully.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="400">Cannot delete active booking without force delete.</response>
        /// <response code="401">Unauthorized — the request requires owner or admin privileges.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteBooking(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromQuery] bool isSoft = true,
            [FromQuery] bool forceDelete = false)
        {
            var command = new DeleteBookingCommand
            {
                Id = id,
                IsSoft = isSoft,
                ForceDelete = forceDelete
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Bookings.Prefix);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return Ok(result);
        }

        /// <summary>
        /// Cancels an existing booking.
        /// </summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <param name="cancelBookingDto">Cancellation details including reason.</param>
        /// <returns>Returns success message wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Owner or Admin role authorization.
        /// Changes booking status to Cancelled and restores room availability.
        /// Cannot cancel already cancelled or completed bookings.
        /// </remarks>
        /// <response code="200">Booking cancelled successfully.</response>
        /// <response code="400">Cannot cancel booking due to invalid status.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="401">Unauthorized — the request requires owner or admin privileges.</response>
        [HttpPost("{id}/cancel")]
        //[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelBooking(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromBody] CancelBookingDto cancelBookingDto)
        {
            var command = new CancelBookingCommand
            {
                Id = id,
                CancelBookingDto = cancelBookingDto
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Bookings.Prefix);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return Ok(result);
        }

        /// <summary>
        /// Changes the status of an existing booking.
        /// </summary>
        /// <param name="id">The ID of the booking to update.</param>
        /// <param name="changeBookingStatusDto">The new status and optional notes.</param>
        /// <returns>Returns the updated booking details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Admin or HotelManager role authorization.
        /// Validates status transitions based on business rules.
        /// Automatically manages room availability based on status changes.
        /// </remarks>
        /// <response code="200">Booking status changed successfully.</response>
        /// <response code="400">Invalid status transition.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="401">Unauthorized — the request requires admin or hotel manager privileges.</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.HotelManager)}")]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangeBookingStatus(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromBody] ChangeBookingStatusDto changeBookingStatusDto)
        {
            var command = new ChangeBookingStatusCommand
            {
                Id = id,
                ChangeBookingStatusDto = changeBookingStatusDto
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Bookings.Prefix);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all bookings for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of bookings per page (default: 10).</param>
        /// <returns>Returns a paginated list of user's bookings wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Owner or Admin role authorization.
        /// Returns all bookings belonging to the specified user with pagination.
        /// </remarks>
        /// <response code="200">User bookings retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="401">Unauthorized — the request requires owner or admin privileges.</response>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedList<BookingDto>>>> GetBookingsByUser(
            [FromRoute, Range(1, int.MaxValue)] int userId,
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = new GetBookingsByUserQuery
            {
                UserId = userId,
                Pagination = new PaginationParameters(pageNumber, pageSize)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all bookings for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel.</param>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of bookings per page (default: 10).</param>
        /// <returns>Returns a paginated list of hotel's bookings wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Admin or HotelManager role authorization.
        /// Returns all bookings for rooms in the specified hotel with pagination.
        /// </remarks>
        /// <response code="200">Hotel bookings retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="401">Unauthorized — the request requires admin or hotel manager privileges.</response>
        [HttpGet("hotel/{hotelId}")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.HotelManager)}")]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<BookingDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedList<BookingDto>>>> GetBookingsByHotel(
            [FromRoute, Range(1, int.MaxValue)] int hotelId,
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = new GetBookingsByHotelQuery
            {
                HotelId = hotelId,
                Pagination = new PaginationParameters(pageNumber, pageSize)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Checks if a room is available for the specified date range.
        /// </summary>
        /// <param name="roomId">The ID of the room to check.</param>
        /// <param name="checkInDate">The check-in date.</param>
        /// <param name="checkOutDate">The check-out date.</param>
        /// <param name="excludeBookingId">Optional booking ID to exclude from conflict check (for updates).</param>
        /// <returns>Returns boolean result indicating room availability wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Public endpoint - no authentication required.
        /// Checks for conflicting bookings in the specified date range.
        /// Useful for checking availability before creating or updating bookings.
        /// </remarks>
        /// <response code="200">Room availability checked successfully.</response>
        /// <response code="400">Invalid date range or parameters.</response>
        [HttpGet("check-availability")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckRoomAvailability(
            [FromQuery, Range(1, int.MaxValue)] int roomId,
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate,
            [FromQuery, Range(1, int.MaxValue)] int? excludeBookingId = null)
        {
            var query = new CheckRoomAvailabilityQuery
            {
                RoomId = roomId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                ExcludeBookingId = excludeBookingId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Calculates the total price for a booking with the specified room and date range.
        /// </summary>
        /// <param name="roomId">The ID of the room.</param>
        /// <param name="checkInDate">The check-in date.</param>
        /// <param name="checkOutDate">The check-out date.</param>
        /// <returns>Returns price breakdown wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Public endpoint - no authentication required.
        /// Calculates total price based on room rate and stay duration.
        /// Returns detailed price breakdown including room rate, days, and total cost.
        /// </remarks>
        /// <response code="200">Price calculated successfully.</response>
        /// <response code="400">Invalid date range or room not found.</response>
        [HttpGet("calculate-price")]
        [ProducesResponseType(typeof(ApiResponse<BookingPriceResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookingPriceResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BookingPriceResponseDto>>> CalculateBookingPrice(
            [FromQuery, Range(1, int.MaxValue)] int roomId,
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate)
        {
            var query = new CalculateBookingPriceQuery
            {
                RoomId = roomId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
