using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ICompareTestCase
    {
        public Task<bool> FinalResult(Request request, string urlJudge0);
    }
}
