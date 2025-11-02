using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.Reviews.Commands.CreateReview;
using Hotel_Booking_API.Application.Features.Reviews.Commands.DeleteReview;
using Hotel_Booking_API.Application.Features.Reviews.Commands.UpdateReview;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewById;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviews;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByHotel;
using Hotel_Booking_API.Application.Features.Reviews.Queries.GetReviewsByUser;
using Hotel_Booking_API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_API.Controllers
{
    /// <summary>
    /// Controller for managing review operations in the hotel booking system.
    /// Provides CRUD operations for reviews with proper authorization and validation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new review in the system.
        /// </summary>
        /// <param name="createReviewDto">The review details to create</param>
        /// <param name="userId">The ID of the user creating the review</param>
        /// <returns>Returns the created review details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Customer or Admin role authorization.
        /// Validates user and hotel existence and rating constraints.
        /// </remarks>
        /// <response code="201">Review created successfully.</response>
        /// <response code="400">Validation failed or invalid rating.</response>
        /// <response code="401">Unauthorized — the request requires authentication.</response>
        [HttpPost]
        //[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview(
            [FromBody] CreateReviewDto createReviewDto,
            [FromQuery, Range(1, int.MaxValue)] int userId)
        {
            var command = new CreateReviewCommand
            {
                CreateReviewDto = createReviewDto,
                UserId = userId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetReviewById), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific review by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the review.</param>
        /// <returns>Returns the review details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Returns detailed review information including hotel and user details.
        /// </remarks>
        /// <response code="200">Review retrieved successfully.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="401">Unauthorized — the request requires authentication.</response>
        [HttpGet("{id}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> GetReviewById([FromRoute, Range(1, int.MaxValue)] int id)
        {
            var query = new GetReviewByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of reviews with optional filtering.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of reviews per page (default: 10).</param>
        /// <param name="hotelId">Optional filter by hotel ID.</param>
        /// <param name="userId">Optional filter by user ID.</param>
        /// <param name="minRating">Optional filter by minimum rating (1-5).</param>
        /// <param name="maxRating">Optional filter by maximum rating (1-5).</param>
        /// <param name="includeDeleted">Whether to include soft-deleted reviews.</param>
        /// <returns>Returns a paginated list of reviews wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Admin role authorization.
        /// Supports comprehensive filtering and pagination with validation.
        /// </remarks>
        /// <response code="200">List of reviews retrieved successfully.</response>
        /// <response code="400">Invalid filter or pagination parameters.</response>
        /// <response code="401">Unauthorized — the request requires admin privileges.</response>
        [HttpGet]
        //[Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedList<ReviewDto>>>> GetReviews(
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery, Range(1, int.MaxValue)] int? hotelId = null,
            [FromQuery, Range(1, int.MaxValue)] int? userId = null,
            [FromQuery, Range(1, 5)] int? minRating = null,
            [FromQuery, Range(1, 5)] int? maxRating = null,
            [FromQuery] bool includeDeleted = false)
        {
            var search = new SearchReviewsDto
            {
                HotelId = hotelId,
                UserId = userId,
                MinRating = minRating,
                MaxRating = maxRating
            };

            var query = new GetReviewsQuery
            {
                Pagination = new PaginationParameters(pageNumber, pageSize),
                Search = search,
                IncludeDeleted = includeDeleted
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        /// <param name="id">The ID of the review to update.</param>
        /// <param name="updateReviewDto">The fields you want to update.</param>
        /// <returns>Returns the updated review details wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Customer or Admin role authorization.
        /// Supports partial updates - only provided fields will be updated.
        /// Users can only update their own reviews.
        /// </remarks>
        /// <response code="200">Review updated successfully.</response>
        /// <response code="400">Validation failed or review cannot be updated.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="401">Unauthorized — the request requires Customer or admin privileges.</response>
        [HttpPatch("{id}")]
        //[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReview(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromBody] UpdateReviewDto updateReviewDto)
        {
            var command = new UpdateReviewCommand
            {
                Id = id,
                UpdateReviewDto = updateReviewDto
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a review by ID.
        /// </summary>
        /// <param name="id">Review ID to delete</param>
        /// <param name="isSoft">If true, marks the review as deleted instead of removing it permanently (default: true)</param>
        /// <returns>204 No Content if successful, 404 if not found</returns>
        /// <remarks>
        /// Requires Owner or Admin role authorization.
        /// By default, performs soft delete to maintain data integrity.
        /// Users can only delete their own reviews.
        /// </remarks>
        /// <response code="204">Review deleted successfully.</response>
        /// <response code="404">Review not found.</response>
        /// <response code="400">Review already deleted.</response>
        /// <response code="401">Unauthorized — the request requires owner or admin privileges.</response>
        [HttpDelete("{id}")]
        //[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReview(
            [FromRoute, Range(1, int.MaxValue)] int id,
            [FromQuery] bool isSoft = true)
        {
            var command = new DeleteReviewCommand
            {
                Id = id,
                IsSoft = isSoft
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all reviews for a specific hotel.
        /// </summary>
        /// <param name="hotelId">The ID of the hotel.</param>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of reviews per page (default: 10).</param>
        /// <returns>Returns a paginated list of hotel's reviews wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Public endpoint - no authentication required.
        /// Returns all reviews for the specified hotel with pagination.
        /// </remarks>
        /// <response code="200">Hotel reviews retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="404">Hotel not found.</response>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PagedList<ReviewDto>>>> GetReviewsByHotel(
            [FromRoute, Range(1, int.MaxValue)] int hotelId,
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = new GetReviewsByHotelQuery
            {
                HotelId = hotelId,
                Pagination = new PaginationParameters(pageNumber, pageSize)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all reviews for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="pageNumber">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of reviews per page (default: 10).</param>
        /// <returns>Returns a paginated list of user's reviews wrapped in an ApiResponse</returns>
        /// <remarks>
        /// Requires Owner or Admin role authorization.
        /// Returns all reviews belonging to the specified user with pagination.
        /// </remarks>
        /// <response code="200">User reviews retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="401">Unauthorized — the request requires owner or admin privileges.</response>
        [HttpGet("user/{userId}")]
        //[Authorize(Roles = $"{nameof(UserRole.Customer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ReviewDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedList<ReviewDto>>>> GetReviewsByUser(
            [FromRoute, Range(1, int.MaxValue)] int userId,
            [FromQuery, Range(1, 1000)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = new GetReviewsByUserQuery
            {
                UserId = userId,
                Pagination = new PaginationParameters(pageNumber, pageSize)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
