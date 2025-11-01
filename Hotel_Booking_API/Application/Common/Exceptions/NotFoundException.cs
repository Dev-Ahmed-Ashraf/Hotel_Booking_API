namespace Hotel_Booking_API.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource was not found.") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string name, object key)
            : base($"{name} with ID {key} was not found.") { }

        public NotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}


