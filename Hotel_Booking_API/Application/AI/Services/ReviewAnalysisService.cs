using Hotel_Booking_API.Application.AI.DTOs;
using Hotel_Booking_API.Application.AI.Prompts;
using Hotel_Booking_API.Application.AI.Services.Groq.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Hotel_Booking_API.Application.AI.Services
{
    public class ReviewAnalysisService : IReviewAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;
        private readonly string _apiKey;

        public ReviewAnalysisService(
            HttpClient httpClient,
        IOptions<GroqSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
                  ?? throw new InvalidOperationException(
                      "GROQ_API_KEY not found.");
        }

        public async Task<ReviewAnalysisResult> AnalyzeAsync(string review)
        {
            // Prepare the prompt by inserting the review into the template
            var prompt = ReviewAnalysisPrompt.ReviewAnalysis.Replace("{{review}}", review);
      
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            // Construct the request body for the Groq API
            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
            new
            {
                role = "user",
                content = prompt
            }
        }
            };

            // Serialize the request body to JSON and send the POST request to the Groq API
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            // Send the request and handle the response
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}chat/completions",
                content);

            
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            
            var groqResponse = JsonSerializer.Deserialize<GroqResponse>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var aiContent = groqResponse?
                .Choices
                .FirstOrDefault()?
                .Message
                .Content;

            if (string.IsNullOrWhiteSpace(aiContent))
                throw new InvalidOperationException("No response received from Groq.");

                        aiContent = aiContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<ReviewAnalysisResult>(
                aiContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null)
                throw new InvalidOperationException("Failed to parse AI response.");

            return result;
        }

    }
}
