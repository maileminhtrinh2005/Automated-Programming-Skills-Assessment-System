using FeedbackService.Application.Dtos;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;

namespace FeedbackService.Application.Services
{
    public class DetailedFeedbackService
    {
        private readonly AppDbContext _db;

        public DetailedFeedbackService(AppDbContext db) // luu db cham chi tiet
        {
            _db = db;
        }

        public async Task SaveDetailedFeedbackAsync(DetailedFeedbackDto dto, CancellationToken ct = default)
        {
            var entity = new DetailedFeedback
            {
                StudentId = dto.StudentId,
                SubmissionId = dto.SubmissionId,
                AssignmentTitle = dto.AssignmentTitle,
                Summary = dto.Summary,
               
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.DetailedFeedbacks.Add(entity);
            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"💾 [DetailedFeedbackService] Saved detailed feedback Id={entity.Id}");
        }
    }
}
