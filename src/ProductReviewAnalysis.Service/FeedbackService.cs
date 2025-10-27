using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data.Models;
using Microsoft.Extensions.Logging;
using ProductReviewAnalysis.Service.AI;

namespace ProductReviewAnalysis.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;
        private readonly IOpenAITextAnalyzer _analyzer;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            IFeedbackRepository repo,
            IOpenAITextAnalyzer analyzer,
            ILogger<FeedbackService> logger)
        {
            _repository = repo ?? throw new ArgumentNullException(nameof(repo));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FeedbackResponseDto> CreateAsync(FeedbackRequestDto createDto)
        {
            try
            {
                if (createDto is null || string.IsNullOrWhiteSpace(createDto.Text))
                    throw new ArgumentException("Text is required", nameof(createDto));

                _logger.LogInformation("Starting feedback analysis for new entry...");

                // Analyze text with AI
                var analysis = await (_analyzer as OpenAITextAnalyzer)?.AnalyzeAsync(createDto.Text)!;

                // Map
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

                var savedReview = await _repository.AddAsync(model);

                _logger.LogInformation("Feedback successfully analyzed and saved. FeedbackId: {FeedbackId}", savedReview.Id);

                return ResponseMapping(savedReview);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input provided when creating feedback.");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Repository operation failed during feedback creation.");
                throw new Exception("Failed to save feedback due to an internal error. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in CreateAsync.");
                throw new Exception("An unexpected error occurred while processing your feedback.", ex);
            }
        }

        public async Task<PagedResult<FeedbackResponseDto>> GetPagedAsync(int page, int pageSize, string? sentiment, string? tag)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 200) pageSize = 20;

                _logger.LogInformation("Fetching paged feedback results. Page: {Page}, Size: {Size}, Sentiment: {Sentiment}, Tag: {Tag}",
                    page, pageSize, sentiment, tag);

                var (items, total) = await _repository.GetPagedAsync(page, pageSize, sentiment, tag);
                var dtoItems = items.Select(ResponseMapping).ToList();

                return new PagedResult<FeedbackResponseDto>
                {
                    Items = dtoItems,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch paged feedback results.");
                throw new Exception("An error occurred while retrieving feedback list.", ex);
            }
        }

        public async Task<FeedbackResponseDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var feedback = await _repository.GetByIdAsync(id);
                if (feedback is null)
                {
                    _logger.LogWarning("Feedback with Id {Id} not found.", id);
                    throw new KeyNotFoundException($"Feedback with Id {id} not found.");
                }

                return ResponseMapping(feedback);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Feedback not found.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving feedback by Id.");
                throw new Exception("An error occurred while retrieving the feedback.", ex);
            }
        }

        private static FeedbackResponseDto ResponseMapping(Feedback f) => new FeedbackResponseDto
        {
            Id = f.Id,
            Text = f.Text,
            Email = f.Email,
            CreatedAt = f.CreatedAt,
            Analysis = new AnalysisDto
            {
                Summary = f.Summary ?? string.Empty,
                Sentiment = f.Sentiment ?? "neutral",
                Tags = f.Tags?.ToList() ?? new List<string>(),
                Priority = f.Priority ?? "P3",
                NextAction = f.NextAction ?? string.Empty
            }
        };
    }
}
