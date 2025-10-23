using Microsoft.EntityFrameworkCore;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data;
using ProductReviewAnalysis.Data.Models;

namespace ProductReviewAnalysis.Repository
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _db;
        public FeedbackRepository(ApplicationDbContext db) => _db = db;

        public async Task<Feedback> AddAsync(Feedback item)
        {
            _db.Feedbacks.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<Feedback?> GetByIdAsync(Guid id)
        {
            return await _db.Feedbacks.FindAsync(id);
        }

        public async Task<(IEnumerable<Feedback> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sentiment, string? tag)
        {
            var q = _db.Feedbacks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(sentiment))
                q = q.Where(f => f.Sentiment == sentiment);

            if (!string.IsNullOrWhiteSpace(tag))
                q = q.Where(f => f.TagsJson != null && EF.Functions.Like(f.TagsJson, $"%{tag}%"));

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }
    }
}
