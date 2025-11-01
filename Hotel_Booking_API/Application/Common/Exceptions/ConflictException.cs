namespace Hotel_Booking_API.Application.Common.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException() : base("A conflict occurred with the current state of the resource.") { }

        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception inner) : base(message, inner) { }
    }
}


