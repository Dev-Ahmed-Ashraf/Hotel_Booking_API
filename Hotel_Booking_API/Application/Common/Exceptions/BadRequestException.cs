namespace Hotel_Booking_API.Application.Common.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("The request is invalid.") { }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(string message, Exception inner) : base(message, inner) { }
    }
}


