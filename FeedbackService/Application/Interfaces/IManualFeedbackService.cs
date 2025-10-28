using FeedbackService.Application.Dtos;

public interface IManualFeedbackService
{
    Task<ManualFeedbackResponseDto> CreateAsync(ManualFeedbackRequestDto dto, CancellationToken ct = default);
    Task SendReviewedFeedbackAsync(ManualFeedbackDto dto);
}
