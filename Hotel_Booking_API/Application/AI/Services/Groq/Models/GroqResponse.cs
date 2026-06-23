namespace Hotel_Booking_API.Application.AI.Services.Groq.Models
{
    public class GroqResponse
    {
        public List<GroqChoice> Choices { get; set; } = new();
    }
}
