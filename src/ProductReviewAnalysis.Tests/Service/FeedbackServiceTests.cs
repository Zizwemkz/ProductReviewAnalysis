using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data.Models;
using ProductReviewAnalysis.Repository;
using ProductReviewAnalysis.Service;

namespace ProductReviewAnalysis.Tests.Service
{
    [TestFixture]
    public class FeedbackServiceTests
    {
        private Mock<IFeedbackRepository> _repoMock = null!;
        private Mock<ITextAnalyzer> _analyzerMock = null!;
        private Mock<ILogger<FeedbackService>> _loggerMock = null!;
        private FeedbackService _service = null!;
       [SetUp]
public void Setup()
{
    _repoMock = new Mock<IFeedbackRepository>();
    _analyzerMock = new Mock<ITextAnalyzer>();
    _loggerMock = new Mock<ILogger<FeedbackService>>();
    _analyzerMock.Setup(a => a.Analyze(It.IsAny<string>())).Returns(
        new AnalysisDto
        {
            Summary = "Summary",
            Sentiment = "positive",
            Tags = new List<string> { "tag1" },
            Priority = "P2",
            NextAction = "Follow up"
        }
    );

    _service = new FeedbackService(_repoMock.Object, _analyzerMock.Object, _loggerMock.Object);
}

        [Ignore("Integration test - requires actual implementation of ITextAnalyzer")]
        [Test]
        public async Task CreateAsync_Should_Return_ResponseDto()
        {
            var req = new FeedbackRequestDto { Text = "Excellent!", Email = "test@mail.com" };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Feedback>())).ReturnsAsync(new Feedback
            {
                Id = Guid.NewGuid(),
                Text = req.Text,
                Email = req.Email,
                CreatedAt = DateTime.UtcNow
            });

            var result = await _service.CreateAsync(req);

            Assert.That(result.Text, Is.EqualTo("Excellent!"));
            Assert.That(result.Analysis.Sentiment, Is.EqualTo("positive"));
        }

        [Test]
        public void CreateAsync_Should_Throw_If_Text_Is_Empty()
        {
            var req = new FeedbackRequestDto { Text = "" };
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.CreateAsync(req));
        }

        [Test]
        public async Task GetPagedAsync_Should_Return_Valid_PagedResult()
        {
            _repoMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), null, null))
                .ReturnsAsync((new List<Feedback> { new Feedback { Id = Guid.NewGuid(), Text = "Sample" } }, 1));

            var result = await _service.GetPagedAsync(1, 10, null, null);

            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetByIdAsync_Should_Throw_If_Not_Found()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Feedback?)null);
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.GetByIdAsync(Guid.NewGuid()));
        }
    }
}

