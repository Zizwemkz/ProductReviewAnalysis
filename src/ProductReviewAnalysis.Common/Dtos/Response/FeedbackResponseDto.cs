
namespace ProductReviewAnalysis.Common.Dtos.Response
{
    public class FeedbackResponseDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public AnalysisDto? Analysis { get; set; }
    }
}
