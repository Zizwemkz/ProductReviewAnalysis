using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProductReviewAnalysis.Data;
using ProductReviewAnalysis.Data.Models;
using ProductReviewAnalysis.Repository;

namespace ProductReviewAnalysis.Tests.Repository
{
    [TestFixture]
    public class FeedbackRepositoryTests
    {
        private ApplicationDbContext _context = null!;
        private FeedbackRepository _repository = null!;
        private Mock<ILogger<FeedbackRepository>> _loggerMock = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<FeedbackRepository>>();
            _repository = new FeedbackRepository(_context, _loggerMock.Object);
        }

        [Test]
        public async Task AddAsync_Should_Add_Feedback_To_Database()
        {
            var feedback = new Feedback { Id = Guid.NewGuid(), Text = "Great product!", CreatedAt = DateTime.UtcNow };
            var result = await _repository.AddAsync(feedback);
            var saved = await _context.Feedbacks.FindAsync(result.Id);

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Text, Is.EqualTo("Great product!"));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Correct_Feedback()
        {
            var f = new Feedback { Id = Guid.NewGuid(), Text = "Feedback1", CreatedAt = DateTime.UtcNow };
            _context.Feedbacks.Add(f);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(f.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Text, Is.EqualTo("Feedback1"));
        }

        [Test]
        public async Task GetPagedAsync_Should_Return_Correct_Number_Of_Items()
        {
            for (int i = 1; i <= 10; i++)
                _context.Feedbacks.Add(new Feedback { Id = Guid.NewGuid(), Text = $"Item{i}", CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var (items, total) = await _repository.GetPagedAsync(1, 5, null, null);

            Assert.That(total, Is.EqualTo(10));
            Assert.That(items.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Null_If_Not_Found()
        {
            var result = await _repository.GetByIdAsync(Guid.NewGuid());
            Assert.That(result, Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}
