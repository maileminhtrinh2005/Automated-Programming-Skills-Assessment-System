using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Services;

public class NotificationGenerator : INotificationGenerator
{
    public async Task<NotificationResponseDto> GenerateFromFeedbackAsync(
        NotificationRequestDto req, CancellationToken ct = default)
    {
      

        await Task.Delay(200, ct); // mô phỏng xử lý async

        // Tạo thông báo đơn giản dựa trên yêu cầu
        var title = $"Feedback cho bài: {req.AssignmentTitle}";
        var message = $"Sinh viên {req.StudentId} đã gửi bài.\n"
                    + $"Ngôn ngữ: {req.LanguageId}\n"
                    + $"Tổng số test: {req.TestResults.Count}";

        return new NotificationResponseDto
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = title,
            Message = message,
            Level = "info",
            CreatedAt = DateTime.UtcNow
        };
    }
}
