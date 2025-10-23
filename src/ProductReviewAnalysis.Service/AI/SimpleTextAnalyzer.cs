using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;

namespace ProductReviewAnalysis.Service.AI
{
    // Simple deterministic analyzer to simulate AI output.
    // Replace with real LLM calls in production (with retries, throttling, observability).
    public class SimpleTextAnalyzer : ITextAnalyzer
    {
        private readonly Dictionary<string, string> _tagKeywords = new()
        {
            { "payment", "payment" },
            { "bug", "bug" },
            { "feature", "feature-request" },
            { "slow", "performance" },
            { "speed", "performance" },
            { "ui", "ux" }
        };

        public AnalysisDto Analyze(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new AnalysisDto { Summary = string.Empty, Sentiment = "neutral", Tags = new List<string>(), Priority = "P3", NextAction = "No action" };

            var lower = text.ToLowerInvariant();

            // summary: first 120 chars or sentence.
            var sentenceEnd = Math.Min(text.IndexOf('.') + 1, text.Length);
            var summary = sentenceEnd > 0 ? text.Substring(0, sentenceEnd).Trim() : (text.Length <= 120 ? text : text.Substring(0, 120) + "...");

            // sentiment simple heuristics
            var positiveWords = new[] { "love", "great", "good", "awesome", "fast", "excellent" };
            var negativeWords = new[] { "fail", "error", "slow", "bug", "bad", "terrible" };

            var posScore = positiveWords.Count(w => lower.Contains(w));
            var negScore = negativeWords.Count(w => lower.Contains(w));

            var sentiment = posScore > negScore ? "positive" : (negScore > posScore ? "negative" : "neutral");

            // tags
            var tags = _tagKeywords.Where(kv => lower.Contains(kv.Key)).Select(kv => kv.Value).Distinct().ToList();

            // priority mapping – presence of payment/bug increases priority
            var priority =
                (lower.Contains("payment") || lower.Contains("fail") || lower.Contains("error")) ? "P0" :
                (lower.Contains("bug") || lower.Contains("crash")) ? "P1" :
                (lower.Contains("feature") || lower.Contains("request")) ? "P2" : "P3";

            // next action: short mapping
            var nextAction = priority switch
            {
                "P0" => "Investigate payments system and contact finance.",
                "P1" => "Assign to engineering for urgent triage.",
                "P2" => "Add to product backlog for prioritization.",
                _ => "Monitor and categorize."
            };

            return new AnalysisDto
            {
                Summary = summary,
                Sentiment = sentiment,
                Tags = tags,
                Priority = priority,
                NextAction = nextAction
            };
        }
    }
}
