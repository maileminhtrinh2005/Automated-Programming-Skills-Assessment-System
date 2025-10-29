using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;
using SubmissionService.Infrastructure.Persistence;

namespace SubmissionService.Infrastructure
{
    public class SubmissionRepository : ISubmissionRepository
    {

        private readonly AppDbContext _context;
        public SubmissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddSubmission(SubmissionDTO submissiondto)
        {
            if (submissiondto == null)
            {
                return -1;
            }
            var submission = new Submission
            {
                AssignmentId = submissiondto.AssignmentId,
                StudentId = submissiondto.StudentId,
                Code = submissiondto.Code,
                LanguageId = submissiondto.LanguageId,
                SubmittedAt = DateTime.Now,
                Status = "Da nop",
                Score = 0
            };

            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();
            Console.WriteLine("cccc" + submission.SubmissionId);
            return submission.SubmissionId;
        }// done

        public async Task<bool> UpdateScore(double score, int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) { return false; }

            submission.Score = score;
            await _context.SaveChangesAsync();
            return true;
        }// done

        public async Task<List<SubmissionDTO>?> GetSubmissionsByStudentId(int id)
        {
            if (id <= 0) return null;
            var submissions = await _context.Submissions.
                Where(s => s.StudentId == id).
                Select(s => new SubmissionDTO
                {
                    SubmissionId = s.SubmissionId,
                    AssignmentId = s.AssignmentId,
                    Code = s.Code,
                    LanguageId = s.LanguageId,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status,
                    Score = s.Score,
                    LanguageName=s.Language.LanguageName
                }).ToListAsync();

            return submissions;
        }
    }
}
