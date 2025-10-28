using FeedbackService.Application.Dtos;
using FeedbackService.Application.Events;
using FeedbackService.Application.Interfaces;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;
using ShareLibrary;                 // Dùng chung IEventBus
using ShareLibrary.Event;           // ✅ Event chung được định nghĩa tại ShareLibrary
using System;

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
                AssignmentTitle = dto.AssignmentTitle,
                InstructorId = dto.InstructorId,
                Score = dto.Score,
                Content = dto.Content,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.ManualFeedbacks.Add(entity);
            await _db.SaveChangesAsync(ct);

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
            Console.WriteLine("=== [FeedbackService] Preparing FeedbackReviewedEvent ===");
            Console.WriteLine($"SubmissionId: {dto.SubmissionId}");
            Console.WriteLine($"StudentId: {dto.StudentId}");
            Console.WriteLine($"FeedbackText: {dto.FeedbackText}");
            Console.WriteLine($"Comment: {dto.Comment}");
            Console.WriteLine("=========================================================");

            var ev = new FeedbackReviewedEvent
            {
                SubmissionId = dto.SubmissionId,
                StudentId = dto.StudentId,     
                InstructorId = dto.InstructorId,        
                AssignmentTitle = "Unknown",
                FeedbackText = dto.FeedbackText,
                Comment = dto.Comment,
                ReviewedAt = DateTime.UtcNow
            };

            Console.WriteLine("=== [FeedbackService] Publishing FeedbackReviewedEvent ===");
            _eventBus.Publish(ev);
            Console.WriteLine($"[FeedbackService] ✅ Published FeedbackReviewedEvent for student {dto.StudentId}");
        }
    }
}
