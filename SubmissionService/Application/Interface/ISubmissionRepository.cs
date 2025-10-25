using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ISubmissionRepository
    {
        public Task<int> AddSubmission(SubmissionDTO submission);

        public Task<bool> UpdateScore(double score, int submissionId);

        public Task<List<SubmissionDTO>?> GetSubmissionsByStudentId(int id);

    }
}
