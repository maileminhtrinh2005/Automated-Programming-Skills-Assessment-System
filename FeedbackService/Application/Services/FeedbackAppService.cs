using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;
using System;
using System.Text.Json;


public class FeedbackAppService : IFeedbackAppService
{
    private readonly IFeedbackGenerator _generator;
    private readonly AppDbContext _db;

    public FeedbackAppService(IFeedbackGenerator generator, AppDbContext db)
    {
        _generator = generator;
        _db = db;
    }

    public async Task<FeedbackResponseDto> GenerateAsync(FeedbackRequestDto req, CancellationToken ct)
    {
        var result = await _generator.GenerateAsync(req, ct);

        var record = new GeneratedFeedbackRecord
        {
            StudentId = req.StudentId,
            AssignmentTitle = req.AssignmentTitle,
            Summary = result.Summary ?? "(no summary)",
            Score = result.Score,
            RawJson = JsonSerializer.Serialize(result),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.GeneratedFeedbacks.Add(record);
        await _db.SaveChangesAsync(ct);

        return result;
    }
}
