
namespace ProductReviewAnalysis.Common.Dtos.Response
{
    public class AnalysisDto
    {
        public string Summary { get; set; } = string.Empty!;
        public string Sentiment { get; set; } = "neutral"!;
        public List<string> Tags { get; set; } = new();
        public string Priority { get; set; } = "P3"!;
        public string NextAction { get; set; } = string.Empty!;
    }
}
