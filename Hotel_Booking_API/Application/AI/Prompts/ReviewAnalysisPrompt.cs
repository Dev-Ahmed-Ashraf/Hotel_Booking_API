namespace Hotel_Booking_API.Application.AI.Prompts
{
    public static class ReviewAnalysisPrompt
    {
        public const string ReviewAnalysis = """
Analyze the hotel review delimited by triple backticks.

Extract:

- sentiment (Positive, Neutral, Negative)
- rating (1 to 5)
- short summary
- main issues
- positive points

Return only valid JSON using this structure:

{
  "sentiment": "",
  "aisummary": "",
  "issues": [],
  "positives": []
}

Do not use markdown formatting.
Do not wrap the response in triple backticks.

Review:
```{{review}}```
""";
    }
}
