using System.Net;
using System.Text.Json;

namespace ProductReviewAnalysis.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Path}", ctx.Request.Path);
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.ContentType = "application/json";
                var corruption = new
                {
                    error = new
                    {
                        message = "An internal error occurred.",
                        detail = ex.Message,
                        traceId = ctx.Items.ContainsKey("X-Correlation-ID") ? ctx.Items["X-Correlation-ID"] : Guid.NewGuid().ToString()
                    }
                };
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(corruption));
            }
        }
    }
}
