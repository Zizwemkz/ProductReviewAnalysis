using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data.Models;

namespace ProductReviewAnalysis.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly ITextAnalyzer _analyzer;

        public FeedbackService(IFeedbackRepository repo, ITextAnalyzer analyzer)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        }

        public async Task<FeedbackResponseDto> CreateAsync(FeedbackRequestDto createDto)
        {
            if (createDto is null || string.IsNullOrWhiteSpace(createDto.Text))
                throw new ArgumentException("Text is required", nameof(createDto));

            // analyze
            var analysis = _analyzer.Analyze(createDto.Text);

            // persist
            var model = new Feedback
            {
                Text = createDto.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(createDto.Email) ? null : createDto.Email.Trim(),
                CreatedAt = DateTime.UtcNow,
                Summary = analysis.Summary,
                Sentiment = analysis.Sentiment,
                Tags = analysis.Tags,
                Priority = analysis.Priority,
                NextAction = analysis.NextAction
            };

            var saved = await _repo.AddAsync(model);

            return MapToDto(saved);
        }

        public async Task<PagedResult<FeedbackResponseDto>> GetPagedAsync(int page, int pageSize, string? sentiment, string? tag)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var (items, total) = await _repo.GetPagedAsync(page, pageSize, sentiment, tag);
            var dtoItems = items.Select(MapToDto);
            return new PagedResult<FeedbackResponseDto> { Items = dtoItems.ToList(), TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<FeedbackResponseDto?> GetByIdAsync(Guid id)
        {
            var f = await _repo.GetByIdAsync(id);
            return f is null ? null : MapToDto(f);
        }

        private static FeedbackResponseDto MapToDto(Feedback f) => new FeedbackResponseDto
        {
            Id = f.Id,
            Text = f.Text,
            Email = f.Email,
            CreatedAt = f.CreatedAt,
            Analysis = new AnalysisDto
            {
                Summary = f.Summary ?? string.Empty,
                Sentiment = f.Sentiment ?? "neutral",
                Tags = f.Tags.ToList(),
                Priority = f.Priority ?? "P3",
                NextAction = f.NextAction ?? string.Empty
            }
        };
    }
}
