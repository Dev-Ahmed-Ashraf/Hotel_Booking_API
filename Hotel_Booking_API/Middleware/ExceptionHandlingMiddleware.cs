using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FluentValidation;
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

            var response = new ProblemDetails();

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Title = "Validation Error";
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Detail = "One or more validation errors occurred.";
                    response.Extensions["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    break;

                case ArgumentException argumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Title = "Bad Request";
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Detail = argumentException.Message;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Title = "Unauthorized";
                    response.Status = (int)HttpStatusCode.Unauthorized;
                    response.Detail = "You are not authorized to perform this action.";
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Title = "Not Found";
                    response.Status = (int)HttpStatusCode.NotFound;
                    response.Detail = "The requested resource was not found.";
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Title = "Internal Server Error";
                    response.Status = (int)HttpStatusCode.InternalServerError;
                    response.Detail = "An error occurred while processing your request.";
                    break;
            }

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}
