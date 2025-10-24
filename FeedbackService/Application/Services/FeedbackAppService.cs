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

    private readonly IEventBus _bus;           

    public FeedbackAppService(
        IFeedbackGenerator generator,
        AppDbContext db,
        IEventBus bus)                       
    {
        _generator = generator;
        _db = db;
        _bus = bus;                           

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


        // ✅ Publish event để NotificationService nhận được
        var evt = new FeedbackGeneratedEvent
        {
            SubmissionId = Guid.NewGuid(),          
            Score = result.Score,
            ResultStatus = "Graded",
            Feedback = result.Summary ?? "(no summary)"
            
        };

        Console.WriteLine("[FeedbackAppService] >>> Publishing FeedbackGeneratedEvent");
        _bus.Publish(evt);
        Console.WriteLine("[FeedbackAppService] ✅ Published FeedbackGeneratedEvent");


        return result;
    }
}
