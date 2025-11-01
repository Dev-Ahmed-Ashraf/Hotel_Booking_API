namespace Hotel_Booking_API.Application.Common.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Authentication is required. Please login to continue.") { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception inner) : base(message, inner) { }
    }
}


