using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ISubmit
    {
        public Task<Result> Submited(Request request, string urlJugde0);
    }
}
