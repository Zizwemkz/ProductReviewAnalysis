using Microsoft.EntityFrameworkCore;
using ProductReviewAnalysis.Common.Interfaces;
using ProductReviewAnalysis.Data;
using ProductReviewAnalysis.Middleware;
using ProductReviewAnalysis.Repository;
using ProductReviewAnalysis.Service;
using ProductReviewAnalysis.Service.AI;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// configuration
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=productreviews.db";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Services
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositories & Services
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// Analyzer (AI stub) - replace with a real LLM client implementation later
builder.Services.AddSingleton<IOpenAITextAnalyzer, OpenAITextAnalyzer>();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowFrontend");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Ensure DB & seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // seed sample if empty
    if (!db.Feedbacks.Any())
    {
        var seed = new[]
        {
            new ProductReviewAnalysis.Data.Models.Feedback { Text = "Love the new UI, very intuitive!", Email = "alice@example.com" },
            new ProductReviewAnalysis.Data.Models.Feedback { Text = "Payment failed twice, please fix.", Email = "bob@example.com" },
            new ProductReviewAnalysis.Data.Models.Feedback { Text = "Feature request: Dark mode would be great", Email = "carol@example.com" },
            new ProductReviewAnalysis.Data.Models.Feedback { Text = "App is too slow on startup.", Email = null }
        };
        db.Feedbacks.AddRange(seed);
        db.SaveChanges();
    }
}

app.Run();
