using ProductReviewAnalysis.Data.Models;

namespace ProductReviewAnalysis.Common.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddAsync(Feedback item);
        Task<Feedback?> GetByIdAsync(Guid id);
        Task<(IEnumerable<Feedback> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sentiment, string? tag);
    }
}
