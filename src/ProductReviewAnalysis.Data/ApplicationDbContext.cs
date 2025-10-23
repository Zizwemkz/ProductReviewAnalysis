using Microsoft.EntityFrameworkCore;
using ProductReviewAnalysis.Data.Models;

namespace ProductReviewAnalysis.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
    }
}
