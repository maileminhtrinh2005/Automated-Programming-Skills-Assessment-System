using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ISendToJudge0
    {
        public Task<ResultDTO?> RunCode(string sourceCode, int languageId, string stdin, string urlJugde0);
    }
}
