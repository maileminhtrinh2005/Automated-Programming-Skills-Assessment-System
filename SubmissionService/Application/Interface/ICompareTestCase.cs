using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ICompareTestCase
    {

        public Task GetTestCase(Request request);

        public Task<double> CompareAndUpdateResult(string resultOutput, string expectedOutput,double weight,int submissionId,int testCaseId,int resultId);

        public Task<int> RunAndAddResult(string sourceCode, int languageId, string stdin,int submissionId);

    }
}
