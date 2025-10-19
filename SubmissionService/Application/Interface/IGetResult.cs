using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface IGetResult
    {
        public Task<List<ResultDTO>> GetYourResult(int studentId, int assignmentId);


        public Task<List<ResultDTO>> GetAllYourSubmissions(int studenstId);
    }
}
