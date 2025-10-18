using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;
using SubmissionService.Infrastructure.Persistence;
using Result = SubmissionService.Domain.Result;

namespace SubmissionService.Infrastructure
{
    public class CompareTestCase : ICompareTestCase
    {
        private readonly ISubmit _submit;
        private readonly AppDbContext _dbcontext;
        public CompareTestCase(ISubmit submit, AppDbContext dbContext)
        {
            _submit = submit;
            _dbcontext = dbContext;
        }
        public async Task<bool> FinalResult(Request request, string urlJudge0)
        {
            try
            {
                Console.WriteLine("check 1");
                if (request == null)
                {
                    Console.WriteLine("request null");
                    return false;
                }

                var result = await _submit.Submited(request, urlJudge0);
                if (result == null)
                {
                    Console.WriteLine("result null");
                    return false;
                }

                Submission submission = new Submission
                {
                    AssignmentId = 1,
                    StudentId = 1,
                    Code = request.SourceCode,
                    LanguageId = request.LanguageId,
                    SubmittedAt = DateTime.Now,
                    Status = result.Status,
                    Score = 10
                };

                Console.WriteLine("Thêm submission...");
                _dbcontext.Submissions.Add(submission);
                await _dbcontext.SaveChangesAsync();
                Console.WriteLine("Đã lưu submission với ID = " + submission.SubmissionId);

                Result results = new Result
                {
                    SubmissionId = submission.SubmissionId,
                    TestCaseId = 1,
                    Passed = true,
                    ExecutionTime = result.ExecutionTime,
                    MemoryUsed = result.MemoryUsed,
                    Output = result.Output,
                    ErrorMessage = result.ErrorMessage
                };

                Console.WriteLine("Thêm result...");
                _dbcontext.Results.Add(results);
                await _dbcontext.SaveChangesAsync();

                Console.WriteLine("Xong tất cả!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi FinalResult: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("➡ Inner: " + ex.InnerException.Message);
                return false;
            }
        }
    }
}
