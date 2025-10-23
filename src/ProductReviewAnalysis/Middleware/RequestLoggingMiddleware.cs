using System.Diagnostics;

namespace ProductReviewAnalysis.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            var sw = Stopwatch.StartNew();
            var requestId = ctx.Request.Headers.ContainsKey("X-Correlation-ID")
                ? ctx.Request.Headers["X-Correlation-ID"].ToString()
                : System.Guid.NewGuid().ToString();

            ctx.Response.OnStarting(() =>
            {
                if (!ctx.Response.Headers.ContainsKey("X-Correlation-ID"))
                    ctx.Response.Headers.Add("X-Correlation-ID", requestId);
                return Task.CompletedTask;
            });

            ctx.Items["X-Correlation-ID"] = requestId;
            _logger.LogInformation("Started {Method} {Path} CorrelationId={CorrelationId}", ctx.Request.Method, ctx.Request.Path, requestId);

            await _next(ctx);

            sw.Stop();
            _logger.LogInformation("Completed {Method} {Path} {StatusCode} in {Elapsed}ms CorrelationId={CorrelationId}",
                ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds, requestId);
        }
    }
}
