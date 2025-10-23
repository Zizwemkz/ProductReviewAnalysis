using Microsoft.AspNetCore.Mvc;
using ProductReviewAnalysis.Common.Dtos.Request;
using ProductReviewAnalysis.Common.Interfaces;

namespace ProductReviewAnalysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { error = "Invalid payload" });

            try
            {
                var created = await _feedbackService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sentiment = null, [FromQuery] string? tag = null)
        {
            var result = await _feedbackService.GetPagedAsync(page, pageSize, sentiment, tag);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var found = await _feedbackService.GetByIdAsync(id);
            if (found == null) return NotFound();
            return Ok(found);
        }
    }
}
