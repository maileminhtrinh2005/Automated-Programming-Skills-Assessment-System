using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ISubmit
    {
        public Task<ResultDTO> Submited(Request request, string urlJugde0);
    }
}
