using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Infrastructure.Persistence;

namespace SubmissionService.Infrastructure
{
    public class GetResult : IGetResult
    {
        private readonly AppDbContext _context;
        public GetResult(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ResultDTO>> GetYourResult(int studentId, int assignmentId)
        {
            var query = from s in _context.Submissions
                        join r in _context.Results
                        on s.SubmissionId equals r.SubmissionId
                        where s.StudentId == studentId && s.AssignmentId == assignmentId
                        orderby s.SubmittedAt descending
                        select new ResultDTO
                        {
                            ResultId= r.ResultId,
                            SubmissionId= s.SubmissionId,
                            AssignmentId= s.AssignmentId,
                            Status = s.Status,
                            Score = s.Score,
                            TestCaseId = r.TestCaseId,
                            Passed= r.Passed,
                            ExecutionTime= r.ExecutionTime,
                            MemoryUsed= r.MemoryUsed,
                            Output=r.Output,
                            ErrorMessage=r.ErrorMessage
                        };
            return await query.ToListAsync();
        }
        public async Task<List<ResultDTO>> GetAllYourSubmissions(int studentId)
        {
            var query = from s in _context.Submissions
                        where s.StudentId == studentId
                        select new ResultDTO
                        {
                            AssignmentId= s.AssignmentId
                        };

            return await query.ToListAsync();
        }
    }
}
