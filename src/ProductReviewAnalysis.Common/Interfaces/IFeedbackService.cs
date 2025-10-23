using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Dtos.Response;

namespace ProductReviewAnalysis.Common.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackResponseDto> CreateAsync(FeedbackRequestDto createDto);
        Task<PagedResult<FeedbackResponseDto>> GetPagedAsync(int page, int pageSize, string? sentiment, string? tag);
        Task<FeedbackResponseDto?> GetByIdAsync(Guid id);

    }
}
