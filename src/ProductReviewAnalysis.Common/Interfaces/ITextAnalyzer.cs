using ProductReviewAnalysis.Common.Dtos.Response;

namespace ProductReviewAnalysis.Common.Interfaces
{
    public interface ITextAnalyzer
    {
        public AnalysisDto Analyze(string text);
    }
}
