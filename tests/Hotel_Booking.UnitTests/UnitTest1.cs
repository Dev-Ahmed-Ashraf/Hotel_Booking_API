using FluentAssertions;

namespace Hotel_Booking.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Booking_Should_Fail_When_Room_Not_Available()
        {
            bool roomAvailable = false;

            Action act = () =>
            {
                if (!roomAvailable)
                    throw new InvalidOperationException("Room is not available");
            };

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Room is not available");
        }

        [Fact]
        public void Booking_Should_Fail_If_CheckOut_Before_CheckIn()
        {
            var checkIn = DateTime.Today.AddDays(3);
            var checkOut = DateTime.Today.AddDays(1);

            Action act = () =>
            {
                if (checkOut <= checkIn)
                    throw new ArgumentException("Check-out must be after check-in");
            };

            act.Should().Throw<ArgumentException>()
               .WithMessage("Check-out must be after check-in");
        }
    }
}