using System.ComponentModel.DataAnnotations.Schema;

namespace ProductReviewAnalysis.Data.Models
{
    public class Feedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Analysis fields stored as JSON in DB (simple)
        public string? Summary { get; set; }
        public string? Sentiment { get; set; }
        public string? TagsJson { get; set; }
        public string? Priority { get; set; }
        public string? NextAction { get; set; }

        [NotMapped]
        public IEnumerable<string> Tags
        {
            get => string.IsNullOrWhiteSpace(TagsJson) ? Array.Empty<string>() : System.Text.Json.JsonSerializer.Deserialize<string[]>(TagsJson)!;
            set => TagsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}
