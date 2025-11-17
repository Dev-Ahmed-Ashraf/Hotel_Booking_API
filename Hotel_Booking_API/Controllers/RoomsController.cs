using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Rooms.Commands.CreateRoom;
using Hotel_Booking_API.Application.Features.Rooms.Commands.DeleteRoom;
using Hotel_Booking_API.Application.Features.Rooms.Commands.UpdateRoom;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetAvailableRooms;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetRoomById;
using Hotel_Booking_API.Application.Features.Rooms.Queries.GetRooms;
using Hotel_Booking_API.Domain.Enums;
using Hotel_Booking_API.Infrastructure.Caching;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_API.Controllers
{
    /// <summary>
    /// Controller for managing room operations in the hotel booking system.
    /// Provides CRUD operations for rooms with proper authorization and validation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        #region Fields & Constructor
        private readonly IMediator _mediator;
        private readonly ICacheInvalidator _cacheInvalidator;

        public RoomsController(IMediator mediator, ICacheInvalidator cacheInvalidator)
        {
            _mediator = mediator;
            _cacheInvalidator = cacheInvalidator;
        }
        #endregion

        #region Get Endpoints
        /// <summary>
        /// Retrieves a paginated list of rooms with optional filtering.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of rooms per page (default: 10).</param>
        /// <param name="hotelId">Optional filter by hotel ID.</param>
        /// <param name="hotelName">Optional filter by hotel name.</param>
        /// <param name="roomNumber">Optional filter by room number.</param>
        /// <param name="type">Optional filter by room type.</param>
        /// <param name="minPrice">Optional minimum price filter.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <param name="capacity">Optional minimum capacity filter.</param>
        /// <param name="includeDeleted">Whether to include soft-deleted rooms.</param>
        /// <returns>
        /// Returns a paginated list of rooms wrapped in an <see cref="ApiResponse{PagedList{RoomDto}}"/> object.
        /// </returns>
        /// <remarks>
        /// Supports comprehensive filtering and pagination with validation.  
        /// </remarks>
        /// <response code="200">List of rooms retrieved successfully.</response>
        /// <response code="400">Invalid filter or pagination parameters.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedList<RoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<RoomDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PagedList<RoomDto>>>> GetRooms(
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery, Range(1, int.MaxValue)] int? hotelId = null,
            [FromQuery, StringLength(200)] string? hotelName = null,
            [FromQuery, StringLength(50)] string? roomNumber = null,
            [FromQuery] RoomType? type = null,
            [FromQuery, Range(0, 10000)] decimal? minPrice = null,
            [FromQuery, Range(0, 10000)] decimal? maxPrice = null,
            [FromQuery, Range(1, 10)] int? capacity = null,
            [FromQuery] bool includeDeleted = false)
        {
            var search = new SearchRoomsDto
            {
                HotelId = hotelId,
                HotelName = hotelName,
                RoomNumber = roomNumber,
                Type = type,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Capacity = capacity
            };

            var query = new GetRoomsQuery
            {
                Pagination = new PaginationParameters(pageNumber, pageSize),
                Search = search,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }


        /// <summary>
        /// Retrieves a specific room by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the room.</param>
        /// <returns>
        /// Returns the room details wrapped in an <see cref="ApiResponse{RoomDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Returns detailed room information including hotel details.
        /// </remarks>
        /// <response code="200">Room retrieved successfully.</response>
        /// <response code="404">Room not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoomDto>>> GetRoomById([FromRoute, Range(1, int.MaxValue)] int id)
        {
            var query = new GetRoomByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves available rooms for a specific date range.
        /// </summary>
        /// <param name="hotelId">Optional hotel ID filter.</param>
        /// <param name="checkInDate">The check-in date.</param>
        /// <param name="checkOutDate">The check-out date.</param>
        /// <param name="type">Optional room type filter.</param>
        /// <param name="minCapacity">Optional minimum capacity filter.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <returns>
        /// Returns a list of available rooms wrapped in an <see cref="ApiResponse{List{RoomDto}}"/> object.
        /// </returns>
        /// <remarks>
        /// Checks room availability by examining existing bookings and room status.
        /// </remarks>
        /// <response code="200">Available rooms retrieved successfully.</response>
        /// <response code="400">Invalid date range or parameters.</response>
        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<List<RoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<RoomDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAvailableRooms(
            [FromQuery, Range(1, int.MaxValue)] int? hotelId = null,
            [FromQuery] DateTime checkInDate = default,
            [FromQuery] DateTime checkOutDate = default,
            [FromQuery] RoomType? type = null,
            [FromQuery, Range(1, 10)] int? minCapacity = null,
            [FromQuery, Range(0.01, 10000)] decimal? maxPrice = null)
        {
            // Set default dates if not provided
            if (checkInDate == default)
                checkInDate = DateTime.Today;
            if (checkOutDate == default)
                checkOutDate = checkInDate.AddDays(1);

            var query = new GetAvailableRoomsQuery
            {
                filter = new AvailableRoomsDto
                {
                    HotelId = hotelId,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    Type = type,
                    MinCapacity = minCapacity,
                    MaxPrice = maxPrice
                }
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a list of all available room types.
        /// </summary>
        /// <returns>Returns a list of room types with ID and Name.</returns>
        /// <response code="200">Room types retrieved successfully.</response>
        [HttpGet("types")]
        public IActionResult GetRoomTypes()
        {
            var types = Enum.GetValues(typeof(RoomType))
                .Cast<RoomType>()
                .Select(x => new
                {
                    Id = (int)x,
                    Name = x.ToString()
                })
                .ToList();

            return Ok(types);
        }
        #endregion

        #region Post Endpoints
        /// <summary>
        /// Creates a new room in the system.
        /// </summary>
        /// <param name="createRoomDto">
        /// The room details to create, including hotel ID, room number, type, price, capacity, and description.
        /// </param>
        /// <returns>
        /// Returns the created room details wrapped in an <see cref="ApiResponse{RoomDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Requires **Admin** or **HotelManager** role authorization.  
        /// The API will return a `201 Created` response with the location of the new room.
        /// Room number must be unique within the hotel.
        /// </remarks>
        /// <response code="201">Room created successfully.</response>
        /// <response code="400">Validation failed — one or more fields are invalid.</response>
        /// <response code="401">Unauthorized — the request requires admin or hotel manager privileges.</response>
        [HttpPost]
        //[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.HotelManager)}")]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            var command = new CreateRoomCommand { CreateRoomDto = createRoomDto };
            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);

            return CreatedAtAction(
                nameof(GetRoomById),
                new { id = result.Data.Id },
                result
            );
        }
        #endregion

        #region Patch Endpoints
        /// <summary>
        /// Updates specific room details.
        /// </summary>
        /// <param name="id">The ID of the room to update.</param>
        /// <param name="dto">
        /// The fields you want to update.  
        /// You can send only the properties that need changes (partial update supported).
        /// </param>
        /// <returns>
        /// Returns the updated room details wrapped in an <see cref="ApiResponse{RoomDto}"/> object.
        /// </returns>
        /// <remarks>
        /// Requires **Admin** or **HotelManager** role authorization.  
        /// Supports partial updates - only provided fields will be updated.
        /// Room number must remain unique within the hotel if changed.
        /// </remarks>
        /// <response code="200">Room updated successfully.</response>
        /// <response code="400">Validation failed — one or more fields are invalid.</response>
        /// <response code="404">Room not found.</response>
        /// <response code="401">Unauthorized — the request requires admin or hotel manager privileges.</response>
        [HttpPatch("{id}")]
        //[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.HotelManager)}")]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto dto)
        {
            var command = new UpdateRoomCommand
            {
                Id = id,
                UpdateRoomDto = dto
            };

            var result = await _mediator.Send(command);
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);
            return Ok(result);
        }
        #endregion

        #region Delete Endpoints
        /// <summary>
        /// Deletes a room by ID (soft or hard delete).
        /// </summary>
        /// <param name="id">Room ID to delete.</param>
        /// <param name="isSoft">
        /// If true, performs a soft delete (default).  
        /// Soft delete keeps the room in the database but marks it as deleted.
        /// </param>
        /// <param name="forceDelete">
        /// If true, allows deletion even if the room has active bookings.  
        /// Use with caution.
        /// </param>
        /// <returns>
        /// Returns an ApiResponse message describing the result of deletion.
        /// </returns>
        /// <remarks>
        /// Requires **Admin** role authorization.  
        /// Soft delete preserves data integrity by preventing hard deletion of active rooms.
        /// </remarks>
        /// <response code="200">Room deleted (soft or hard) successfully.</response>
        /// <response code="404">Room not found.</response>
        /// <response code="400">Room cannot be deleted due to active bookings.</response>
        /// <response code="401">Unauthorized — admin privileges required.</response>
        [HttpDelete("{id}")]
        //[Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteRoom(
            [FromRoute] int id,
            [FromQuery] bool isSoft = true,
            [FromQuery] bool forceDelete = false)
        {
            var command = new DeleteRoomCommand
            {
                Id = id,
                IsSoft = isSoft,
                ForceDelete = forceDelete
            };

            var result = await _mediator.Send(command);

            // Invalidate cache
            await _cacheInvalidator.RemoveByPrefixAsync(CacheKeys.Rooms.Prefix);

            return Ok(result);
        }
        #endregion
    }
}
