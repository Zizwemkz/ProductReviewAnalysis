using System.Text;
using Microsoft.Extensions.Logging;
using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;
using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;

namespace ProductReviewAnalysis.Service.AI
{
    /// <summary>
    /// AI-powered text analyzer that uses Google Gemini (or OpenAI equivalent)
    /// to extract sentiment, summary, and tags from user feedback.
    /// </summary>
    public class OpenAITextAnalyzer : IOpenAITextAnalyzer
    {
        private readonly ILogger<OpenAITextAnalyzer> _logger;
        private readonly IConfiguration _config;

        public OpenAITextAnalyzer(ILogger<OpenAITextAnalyzer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public AnalysisDto Analyze(string text)
        {
            // This method must be synchronous due to interface constraints.
            // If desired, you can change ITextAnalyzer to use Task<AnalysisDto>.
            return AnalyzeAsync(text).GetAwaiter().GetResult();
        }

        public async Task<AnalysisDto> AnalyzeAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new AnalysisDto
                {
                    Summary = "Empty feedback provided.",
                    Sentiment = "neutral",
                    Tags = new List<string>(),
                    Priority = "P3",
                    NextAction = "No action required."
                };
            }

            try
            {
                var apiKey = _config["GoogleAIKey"] ?? throw new InvalidOperationException("Missing Google AI API key.");
                var client = new GoogleAI(apiKey);

                var model = client.GenerativeModel(model: "gemini-2.5-flash");

                _logger.LogInformation("Sending text to Gemini model for AI analysis.");

                // Construct a clear prompt for structured AI output
                var prompt = $@"
                                Analyze the following user product feedback and return a structured JSON response
                                with the following keys:
                                summary, sentiment (positive|neutral|negative), tags (list), priority (P0, P1, P2,P3), nextAction. 
                                User Feedback: { text}";

                var response = await model.GenerateContent(prompt);
                var aiText = response.Text?.Trim();

                if (string.IsNullOrEmpty(aiText))
                {
                    _logger.LogWarning("Gemini returned an empty response for text: {Text}", text);
                    return new AnalysisDto
                    {
                        Summary = text.Length > 120 ? text[..120] + "..." : text,
                        Sentiment = "neutral",
                        Tags = new List<string>(),
                        Priority = "P3",
                        NextAction = "Manual review required."
                    };
                }

                _logger.LogInformation("AI response received successfully.");

                // Attempt to parse as JSON-like output if possible
                var dto = TryParseJsonLikeResponse(aiText) ?? new AnalysisDto
                {
                    Summary = aiText,
                    Sentiment = "neutral",
                    Tags = new List<string>(),
                    Priority = "P3",
                    NextAction = "Review manually."
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI analysis failed for text input: {Text}", text);
                return new AnalysisDto
                {
                    Summary = text.Length > 100 ? text[..100] + "..." : text,
                    Sentiment = "neutral",
                    Tags = new List<string>(),
                    Priority = "P3",
                    NextAction = "Retry AI analysis later."
                };
            }
        }

        private AnalysisDto? TryParseJsonLikeResponse(string aiResponse)
        {
            try
            {
                // Simple JSON extraction: look for field patterns
                var summary = Extract(aiResponse, "summary");
                var sentiment = Extract(aiResponse, "sentiment");
                var priority = Extract(aiResponse, "priority");
                var nextAction = Extract(aiResponse, "nextAction");
                var tags = Extract(aiResponse, "tags")?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()
                           ?? new List<string>();

                return new AnalysisDto
                {
                    Summary = summary ?? string.Empty,
                    Sentiment = sentiment ?? "neutral",
                    Priority = priority ?? "P3",
                    NextAction = nextAction ?? string.Empty,
                    Tags = tags
                };
            }
            catch
            {
                return null;
            }
        }

        private string? Extract(string input, string key)
        {
            var lower = input.ToLower();
            var idx = lower.IndexOf(key.ToLower());
            if (idx == -1) return null;

            var start = input.IndexOf(':', idx);
            if (start == -1) return null;

            var end = input.IndexOfAny(new[] { ',', '\n', '}' }, start + 1);
            return end == -1 ? input[(start + 1)..].Trim(' ', '"', '\'') : input[(start + 1)..end].Trim(' ', '"', '\'');
        }
    }
}
