using FeedbackService.Application.Dtos;
using FeedbackService.Application.Services;

namespace FeedbackService.Application.Interfaces;


public interface IFeedbackAppService
{

    Task<FeedbackResponseDto> GenerateAsync(
         FeedbackRequestDto request,
         string systemPrompt,
         CancellationToken ct);
    Task<object> GenerateBulkFeedbackAsync(BulkFeedbackRequestDto dto, CancellationToken ct);

    Task<GeneratedFeedbackDto> GenerateForStudentAssignmentAsync(
        int studentId,
        int assignmentId,
        CancellationToken ct);
    
}

// Chấm theo từng testcase (đã tách controller riêng)
public interface ITestcaseFeedbackGenerator
{
    Task<FeedbackResponseDto> GenerateAsync(
        TestcaseFeedbackRequestDto req,
        string prompt,
        CancellationToken ct);

}