using Hotel_Booking.Application.Features.Hotels.Commands.UpdateHotel;
using Hotel_Booking.Application.Features.Hotels.Queries.GetHotelById;
using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Hotels.Commands.CreateHotel;
using Hotel_Booking_API.Application.Features.Hotels.Commands.DeleteHotel;
using Hotel_Booking_API.Application.Features.Hotels.Queries.GetHotels;
using Hotel_Booking_API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;

namespace Hotel_Booking_API.Controllers
{
    /// <summary>
    /// Controller for managing Hotel operations in the hotel booking system.
    /// Provides CRUD operations for Hotels with proper authorization and validation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICacheInvalidator _cacheInvalidator;

        public HotelsController(IMediator mediator, ICacheInvalidator cacheInvalidator)
        {
            _mediator = mediator;
            _cacheInvalidator = cacheInvalidator;
        }

        /// <summary>
        /// Retrieves a paginated list of hotels with optional filtering by city, country, or rating.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of hotels per page (default: 10).</param>
        /// <param name="city">Optional filter by city name.</param>
        /// <param name="country">Optional filter by country name.</param>
        /// <param name="minRating">Optional minimum rating filter (e.g., 3).</param>
        /// <param name="maxRating">Optional maximum rating filter (e.g., 5).</param>
        /// <param name="includeDeleted">Whether to include soft-deleted hotels.</param>
        /// <returns>
        /// Returns a paginated list of hotels wrapped in an <see cref="ApiResponse{PagedList{HotelDto}}"/> object.
        /// </returns>
        /// <remarks>
        /// Supports comprehensive filtering and pagination with validation.  
        /// Example request:  
        /// `GET /api/hotels?pageNumber=1&pageSize=10&city=Cairo&minRating=3`
        /// </remarks>
        /// <response code="200">List of hotels retrieved successfully.</response>
        /// <response code="400">Invalid filter or pagination parameters.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedList<HotelDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<HotelDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedList<HotelDto>>>> GetHotels(
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery, StringLength(100)] string? city = null,
            [FromQuery, StringLength(100)] string? country = null,
            [FromQuery, Range(0, 5)] decimal? minRating = null,
            [FromQuery, Range(0, 5)] decimal? maxRating = null,
            [FromQuery] bool includeDeleted = false)
        {
            var search = new SearchHotelsDto
            {
                City = city,
                Country = country,
                MinRating = minRating,
                MaxRating = maxRating
            };

            var query = new GetHotelsQuery
            {
                Pagination = new PaginationParameters(pageNumber, pageSize),
                Search = search,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }


        /// <summary>
        /// Creates a new hotel in the system.
        /// </summary>
        /// <param name="createHotelDto">
        /// The hotel details to create, including name, description, address, city, country, and rating.
        /// </param>
        /// <returns>
        /// Returns the created hotel details wrapped in an <see cref="ApiResponse{HotelDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Requires **Admin** role authorization.  
        /// The API will return a `201 Created` response with the location of the new hotel.
        /// </remarks>
        /// <response code="201">Hotel created successfully.</response>
        /// <response code="400">Validation failed — one or more fields are invalid.</response>
        /// <response code="401">Unauthorized — the request requires admin privileges.</response>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<HotelDto>>> CreateHotel([FromBody] CreateHotelDto createHotelDto)
        {
            var command = new CreateHotelCommand { CreateHotelDto = createHotelDto };
            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Hotels.Prefix);
            return CreatedAtAction(nameof(GetHotels), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific hotel by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the hotel.</param>
        /// <returns>
        /// Returns the hotel details wrapped in an <see cref="ApiResponse{HotelDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Returns detailed hotel information including room statistics.
        /// </remarks>
        /// <response code="200">Hotel retrieved successfully.</response>
        /// <response code="404">Hotel not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<HotelDto>>> GetHotelById([FromRoute, Range(1, int.MaxValue)] int id)
        {
            var query = new GetHotelByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }


        /// <summary>
        /// Updates specific hotel details.
        /// </summary>
        /// <param name="id">The ID of the hotel to update.</param>
        /// <param name="dto">
        /// The fields you want to update.  
        /// You can send only the properties that need changes (partial update supported).
        /// </param>
        /// <returns>
        /// Returns the updated hotel details wrapped in an <see cref="ApiResponse{HotelDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Requires **Admin** or **HotelManager** role authorization.  
        /// Supports partial updates - only provided fields will be updated.
        /// Hotel name must remain unique if changed.
        /// </remarks>
        /// <response code="200">Hotel updated successfully.</response>
        /// <response code="400">Validation failed — one or more fields are invalid.</response>
        /// <response code="404">Hotel not found.</response>
        /// <response code="401">Unauthorized — the request requires admin or hotel manager privileges.</response>
        [HttpPatch("{id}")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.HotelManager)}")]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateHotel([FromRoute, Range(1, int.MaxValue)] int id, [FromBody] UpdateHotelDto dto)
        {
            var command = new UpdateHotelCommand
            {
                Id = id,
                UpdateHotelDto = dto
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Hotels.Prefix);
            return Ok(result);
        }


        /// <summary>
        /// Deletes a hotel by ID.
        /// </summary>
        /// <param name="id">Hotel ID to delete</param>
        /// <param name="isSoft">If true, marks the hotel as deleted instead of removing it permanently (default: true)</param>
        /// <param name="forceDelete">If true, forces deletion even if hotel has active bookings (default: false)</param>
        /// <returns>204 No Content if successful, 404 if not found</returns>
        /// <remarks>
        /// Requires **Admin** role authorization.  
        /// By default, performs soft delete to maintain data integrity.
        /// Cannot delete hotels with active bookings unless forceDelete is true.
        /// </remarks>
        /// <response code="204">Hotel deleted successfully.</response>
        /// <response code="404">Hotel not found.</response>
        /// <response code="400">Cannot delete hotel with active bookings.</response>
        /// <response code="401">Unauthorized — the request requires admin privileges.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteHotel([FromRoute, Range(1, int.MaxValue)] int id, [FromQuery] bool isSoft = true, [FromQuery] bool forceDelete = false)
        {
            var command = new DeleteHotelCommand { Id = id, IsSoft = isSoft, ForceDelete = forceDelete };
            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Hotels.Prefix);
            return NoContent();
        }
    }
}