using System.ComponentModel.DataAnnotations;

namespace ProductReviewAnalysis.Common.Dtos.Request
{
    public class FeedbackRequestDto
    {
        [Required]
        public string Text { get; set; } = null!;
        public string? Email { get; set; }
    }
}
