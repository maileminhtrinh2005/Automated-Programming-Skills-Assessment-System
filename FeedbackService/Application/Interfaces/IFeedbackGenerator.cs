using FeedbackService.Application.Dtos;

namespace FeedbackService.Application.Interfaces;

public interface IFeedbackGenerator
{
    Task<FeedbackResponseDto> GenerateAsync(
        FeedbackRequestDto req,
        string prompt,
        CancellationToken ct);
}

