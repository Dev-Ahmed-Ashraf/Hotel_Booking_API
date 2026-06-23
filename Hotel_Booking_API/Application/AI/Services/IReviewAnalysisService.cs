using Hotel_Booking_API.Application.AI.DTOs;

namespace Hotel_Booking_API.Application.AI.Services
{
    public interface IReviewAnalysisService
    {
        Task<ReviewAnalysisResult> AnalyzeAsync(string review);
    }
}
