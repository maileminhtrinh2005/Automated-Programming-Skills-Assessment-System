using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Application.Constants;

namespace FeedbackService.Application.Services
{
    public class TestcaseFeedbackGenerator : ITestcaseFeedbackGenerator
    {
        private readonly ITestcaseFeedbackGenerator _geminiGenerator;

        // ✅ Inject GeminiTestcaseFeedbackGenerator qua DI
        public TestcaseFeedbackGenerator(ITestcaseFeedbackGenerator geminiGenerator)
        {
            _geminiGenerator = geminiGenerator;
        }

        public async Task<FeedbackResponseDto> GenerateAsync(
            TestcaseFeedbackRequestDto req,
            string prompt,
            CancellationToken ct)
        {
            if (req.TestResults == null || req.TestResults.Count == 0)
                throw new ArgumentException("Thiếu TestResults để chấm chi tiết.");

            // ✅ Gọi Gemini thật để sinh nhận xét
            return await _geminiGenerator.GenerateAsync(req, Prompt.ProgressFeedback, ct);
        }
    }
}
