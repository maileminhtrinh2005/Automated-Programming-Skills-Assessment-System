using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface IResultHandle
    {
        public Task<List<ResultDTO>> GetYourResult(int studentId, int assignmentId);


        public Task<List<ResultDTO>> GetAllYourSubmissions(int studenstId);

        public Task<int> AddResult(ResultDTO result);

        public Task<ResultDTO?> GetResultById(int id);

        public Task<bool> UpdateResult (int resultId,int submissionId, int testCaseId, bool passed);
    }
}
