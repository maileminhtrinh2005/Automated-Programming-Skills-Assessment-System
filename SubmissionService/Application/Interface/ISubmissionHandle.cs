using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ISubmissionHandle
    {
        public Task<int> AddSubmission(SubmissionDTO submission);

        public Task<bool> UpdateScore(double score, int submissionId);
    }
}
