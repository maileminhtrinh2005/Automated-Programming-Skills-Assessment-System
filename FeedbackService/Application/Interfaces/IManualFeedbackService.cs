using FeedbackService.Application.Dtos;

namespace FeedbackService.Application.Interfaces;

public interface IManualFeedbackService
{
    Task<ManualFeedbackResponseDto> CreateAsync(ManualFeedbackRequestDto dto, CancellationToken ct = default);
}
