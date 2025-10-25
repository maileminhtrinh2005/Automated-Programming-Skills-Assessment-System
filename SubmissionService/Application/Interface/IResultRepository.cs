using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface IResultRepository
    {
        public Task<List<ResultDTO>> GetResults( int submissionId);


        public Task<List<ResultDTO>> GetAllYourSubmissions(int studenstId);

        public Task<int> AddResult(ResultDTO result);// done

        public Task<ResultDTO?> GetResultById(int id);//done

        public Task<bool> UpdateResult (int resultId,int submissionId, int testCaseId, bool passed);//done
    }
}
