namespace Hotel_Booking_API.Application.AI.DTOs
{
    public class ReviewAnalysisResult
    {
        public string? Sentiment { get; set; }

        //public int? Rating { get; set; }

        public string? AISummary { get; set; }

        public List<string>? issues { get; set; }

        public List<string>? positives { get; set; }
    }
}
