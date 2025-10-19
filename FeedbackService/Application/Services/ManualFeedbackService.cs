using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;
using System;

namespace FeedbackService.Application.Services;

public class ManualFeedbackService : IManualFeedbackService
{
    private readonly AppDbContext _db;
    public ManualFeedbackService(AppDbContext db) => _db = db;

    public async Task<ManualFeedbackResponseDto> CreateAsync(ManualFeedbackRequestDto dto, CancellationToken ct = default)
    {
        var entity = new ManualFeedback
        {
            StudentId = dto.StudentId,
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
            StudentId = entity.StudentId,
            AssignmentTitle = entity.AssignmentTitle,
            InstructorId = entity.InstructorId,
            Score = entity.Score,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
