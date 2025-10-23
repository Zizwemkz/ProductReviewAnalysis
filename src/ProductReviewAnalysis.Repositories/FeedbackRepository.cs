using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data;
using ProductReviewAnalysis.Data.Models;

namespace ProductReviewAnalysis.Repository
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FeedbackRepository> _logger;

        public FeedbackRepository(ApplicationDbContext db, ILogger<FeedbackRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Feedback> AddAsync(Feedback userReview)
        {
            try
            {
                if (userReview == null)
                {
                    throw new ArgumentNullException(nameof(userReview));
                }
                    
                _db.Feedbacks.Add(userReview);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Successfully saved feedback with Id={Id}", userReview.Id);

                return userReview;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database insert failed while adding feedback with Text='{Text}'", userReview?.Text);
                throw new Exception("Failed to save feedback to the database. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding feedback.");
                throw;
            }
        }

        public async Task<Feedback?> GetByIdAsync(Guid id)
        {
            try
            {
                var feedback = await _db.Feedbacks.FindAsync(id);

                if (feedback == null)
                {
                    _logger.LogWarning("Feedback not found for Id={Id}", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved feedback with Id={Id}", id);
                }

                return feedback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching feedback by Id={Id}", id);
                throw new Exception("An error occurred while retrieving feedback details.", ex);
            }
        }

        public async Task<(IEnumerable<Feedback> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? sentiment, string? tag)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching paged feedback list. Page={Page}, PageSize={PageSize}, Sentiment={Sentiment}, Tag={Tag}",
                    page, pageSize, sentiment, tag);

                var q = _db.Feedbacks.AsQueryable();

                if (!string.IsNullOrWhiteSpace(sentiment))
                {
                    q = q.Where(f => f.Sentiment == sentiment);
                    _logger.LogDebug("Applied sentiment filter: {Sentiment}", sentiment);
                }

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    q = q.Where(f => f.TagsJson != null && EF.Functions.Like(f.TagsJson, $"%{tag}%"));
                    _logger.LogDebug("Applied tag filter: {Tag}", tag);
                }

                var total = await q.CountAsync();
                var items = await q
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} feedback entries out of total {TotalCount} (Page={Page})",
                    items.Count, total, page);

                return (items, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to retrieve paged feedback list (Page={Page}, Size={Size}, Sentiment={Sentiment}, Tag={Tag})",
                    page, pageSize, sentiment, tag);
                throw new Exception("An error occurred while retrieving feedback list from the database.", ex);
            }
        }
    }
}
