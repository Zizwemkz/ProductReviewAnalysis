using Microsoft.AspNetCore.Mvc;
using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Interfaces;

namespace ProductReviewAnalysis.Controllers
{
    [ApiController]
    [Route("api")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [HttpPost("feedback")]
        public async Task<ActionResult> Create([FromBody] FeedbackRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { error = "Invalid payload" });

            try
            {
                var result = await _feedbackService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sentiment = null, [FromQuery] string? tag = null)
        {
            var result = await _feedbackService.GetPagedAsync(page, pageSize, sentiment, tag);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest(new { error = "Invalid ID" });
            var result = await _feedbackService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
