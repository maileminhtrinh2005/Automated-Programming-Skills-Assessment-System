using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;

namespace SubmissionService.Application.Service
{
    public class ResultManager
    {
        private readonly IResultRepository _resultRepo;
        public ResultManager (IResultRepository resultRepo)
        {
            _resultRepo = resultRepo;
        }

        public async Task<List<ResultDTO>?> GetResultsBySubmissionId(int submissionId)
        {
            if (submissionId<= 0) return null;

            var results= await _resultRepo.GetResults(submissionId);
            if (results== null)
            {
                return null;
            }
            return results;
        }
    }
}
