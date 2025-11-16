using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Application.Services
{
    public class ManualFeedbackService : IManualFeedbackService
    {
        private readonly AppDbContext _db;
        private readonly IEventBus _eventBus;

        public ManualFeedbackService(AppDbContext db, IEventBus eventBus)
        {
            _db = db;
            _eventBus = eventBus;
        }


        public async Task<ManualFeedbackResponseDto> CreateAsync(ManualFeedbackRequestDto dto, CancellationToken ct = default)
        {
            var entity = new ManualFeedback
            {
                AssignmentTitle = dto.AssignmentTitle ?? "(Không rõ bài tập)",
                InstructorId = dto.InstructorId,
                Score = dto.Score,
                Content = dto.Content,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.ManualFeedbacks.Add(entity);
            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"💾 [FeedbackService] Saved ManualFeedback Id={entity.Id}");

            return new ManualFeedbackResponseDto
            {
                Id = entity.Id,
                StudentId = dto.StudentId,
                AssignmentTitle = entity.AssignmentTitle,
                InstructorId = entity.InstructorId,
                Score = entity.Score,
                Content = entity.Content,
                CreatedAtUtc = entity.CreatedAtUtc
            };
        }

  
        public async Task SendReviewedFeedbackAsync(ManualFeedbackDto dto)
        {
            Console.WriteLine("=== [FeedbackService] Preparing Manual Feedback ===");
            Console.WriteLine($"SubmissionId: {dto.SubmissionId}");
            Console.WriteLine($"StudentId: {dto.StudentId}");
            Console.WriteLine($"FeedbackText: {dto.FeedbackText}");
            Console.WriteLine($"Comment: {dto.Comment}");
            Console.WriteLine("===================================================");

  
            var manualEntity = new ManualFeedback
            {
                AssignmentTitle = dto.AssignmentTitle ?? "(Không rõ bài tập)",
                Score =  0,
                Content = $"{dto.FeedbackText}\nGhi chú: {dto.Comment}",
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.ManualFeedbacks.Add(manualEntity);
            await _db.SaveChangesAsync();

            Console.WriteLine($"💾 [FeedbackService] Manual feedback saved (Id={manualEntity.Id})");


            var ev = new FeedbackGeneratedEvent
            {
                StudentId = dto.StudentId,
                SubmissionId = dto.SubmissionId,
                Score = 0,
                Title = $"Giảng viên đã gửi nhận xét cho bài nộp #{dto.SubmissionId}",
                Message = $"{dto.FeedbackText}\nGhi chú: {dto.Comment}",
                CreatedAtUtc = DateTime.UtcNow
            };

            _eventBus.Publish(ev);

            Console.WriteLine($"📢 [FeedbackService] ✅ Published FeedbackGeneratedEvent for student {dto.StudentId}");
            await Task.CompletedTask;
        }
    }
}
