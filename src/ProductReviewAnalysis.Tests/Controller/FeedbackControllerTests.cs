using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Dtos.Response;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Controllers;
using ProductReviewAnalysis.Repository;

namespace ProductReviewAnalysis.Tests.Controller
{
    [TestFixture]
    public class FeedbackControllerTests
    {
        private Mock<IFeedbackService> _serviceMock = null!;
        private FeedbackController _controller = null!;
        private Mock<ILogger<FeedbackController>> _loggerMock = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IFeedbackService>();
            _loggerMock = new Mock<ILogger<FeedbackController>>();
            _controller = new FeedbackController(_serviceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Post_Should_Return_Ok_With_Valid_Response()
        {
            var dto = new FeedbackRequestDto { Text = "Awesome!" };
            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(new FeedbackResponseDto { Text = "Awesome!" });

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
            var ok = (CreatedAtActionResult)result!;
            Assert.That(((FeedbackResponseDto)ok.Value!).Text, Is.EqualTo("Awesome!"));
        }

        [Test]
        public async Task Get_Should_Return_Paged_Result()
        {
            _serviceMock.Setup(s => s.GetPagedAsync(1, 10, null, null))
                .ReturnsAsync(new PagedResult<FeedbackResponseDto>
                {
                    Items = new List<FeedbackResponseDto> { new FeedbackResponseDto { Text = "Review1" } },
                    TotalCount = 1
                });

            var result = await _controller.GetAll(1, 10, null, null);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result!;
            var value = (PagedResult<FeedbackResponseDto>)ok.Value!;
            Assert.That(value.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetById_Should_Return_NotFound_When_Null()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((FeedbackResponseDto?)null);
            var result = await _controller.GetById(Guid.NewGuid());
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetById_Should_Return_Ok_When_Exists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync(new FeedbackResponseDto { Id = id, Text = "Sample" });

            var result = await _controller.GetById(id);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }
    }
}
