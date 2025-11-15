using FluentValidation;
using Hotel_Booking_API.Application.Common.Exceptions;
using System.Net;

namespace Hotel_Booking_API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var title = "Internal Server Error";
            var message = "An error occurred while processing your request.";
            object? errors = null;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Validation Error";
                    message = "One or more validation errors occurred.";
                    errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    break;

                case BadRequestException bre:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Bad Request";
                    message = bre.Message;
                    break;

                case UnauthorizedException ue:
                case UnauthorizedAccessException ue2:
                    {
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        title = "Unauthorized";
                        message = (exception as Exception).Message;
                        break;
                    }
                case ForbiddenException fe:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    title = "Forbidden";
                    message = fe.Message;
                    break;

                case NotFoundException nfe:
                case KeyNotFoundException knf:
                    {
                        statusCode = (int)HttpStatusCode.NotFound;
                        title = "Not Found";
                        message = (exception as Exception).Message;
                        break;
                    }
                case ConflictException ce:
                    statusCode = (int)HttpStatusCode.Conflict;
                    title = "Conflict";
                    message = ce.Message;
                    break;

                case ArgumentException ae:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Bad Request";
                    message = ae.Message;
                    break;
            }

            context.Response.StatusCode = statusCode;

            var envelope = new
            {
                success = false,
                status = statusCode,
                title,
                message,
                errors
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(envelope));
        }
    }
}
