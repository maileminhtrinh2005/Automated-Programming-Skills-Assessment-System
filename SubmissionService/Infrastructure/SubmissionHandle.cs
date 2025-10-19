using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;
using SubmissionService.Infrastructure.Persistence;

namespace SubmissionService.Infrastructure
{
    public class SubmissionHandle : ISubmissionHandle
    {

        private readonly AppDbContext _context;
        public SubmissionHandle(AppDbContext context)
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
                StudentId = 1, // tam thoi chua co
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
        }

        public async Task<bool> UpdateScore(double score, int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) { return false; }

            submission.Score = score;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
