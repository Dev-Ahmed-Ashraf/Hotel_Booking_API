using Hotel_Booking_API.Application.Common;
using Hotel_Booking_API.Application.DTOs;
using Hotel_Booking_API.Application.Features.AdminDashboard.Queries;
using Hotel_Booking_API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace Hotel_Booking_API.Controllers
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboard()
        {
            var sw = Stopwatch.StartNew();

            var dto = await _mediator.Send(new GetDashboardStatsQuery());

            sw.Stop();
            Log.Information("Dashboard generated in {Elapsed} ms", sw.ElapsedMilliseconds);

            return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(dto));
        }
    }
}


