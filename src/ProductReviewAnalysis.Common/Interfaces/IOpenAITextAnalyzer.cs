using ProductReviewAnalysis.Common.Dtos.Response;

namespace ProductReviewAnalysis.Common.Interfaces
{
    public interface IOpenAITextAnalyzer
    {
        public Task<AnalysisDto> AnalyzeAsync(string text);
    }
}
