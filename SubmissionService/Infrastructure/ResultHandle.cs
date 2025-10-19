using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;
using SubmissionService.Infrastructure.Persistence;

namespace SubmissionService.Infrastructure
{
    public class ResultHandle : IResultHandle
    {
        private readonly AppDbContext _context;
        public ResultHandle(AppDbContext context)
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


        public async Task<int> AddResult(ResultDTO resultdto)
        {
            if (resultdto == null) return -1;

            var result = new Result
            {
                SubmissionId = resultdto.SubmissionId,
                TestCaseId =resultdto.TestCaseId,
                Passed= false,
                ExecutionTime= resultdto.ExecutionTime,
                MemoryUsed= resultdto.MemoryUsed, 
                Output= resultdto.Output,
                ErrorMessage= resultdto.ErrorMessage
            };
            await _context.Results.AddAsync(result);
            await _context.SaveChangesAsync();

            return result.ResultId;
        }

        public async Task<ResultDTO?> GetResultById(int id) // co the nen la string? 
        {
            if (id < 0)
            {
                return null;
            }
            var result = await _context.Results.FirstOrDefaultAsync(r=>r.ResultId==id);
            if (result == null) return null;

            var resultdto = new ResultDTO
            {
                Output=result.Output,
                Passed=result.Passed
            };

            return resultdto;
        }

        public async Task<bool> UpdateResult(int resultId, int submissionId, int testCaseId, bool passed)
        {
            var result = await _context.Results.FirstOrDefaultAsync(r=> r.ResultId==resultId);
            if (result == null) return false;

            result.SubmissionId= submissionId;
            result.TestCaseId= testCaseId;
            result.Passed= passed;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
