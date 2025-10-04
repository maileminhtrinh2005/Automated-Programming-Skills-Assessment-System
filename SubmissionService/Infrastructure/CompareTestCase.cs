using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;

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
            var result = await _submit.Submited(request, urlJudge0);
            if (result == null) { return false; }

            Submission submission = new Submission
            {
                AssignmentId = 1,// tam thoi chua co nen gan mac dinh
                StudentId = 1,// tam thoi chua co
                Code = result.Output,
                Language = "c++", // tam thoi cung chua co
                SubmittedAt = DateTime.Now,
                Status = "Completed",
                Score = 10
            };
            _dbcontext.submissions.Add(submission);
            await _dbcontext.SaveChangesAsync();

            return true;
        }
    }
}
