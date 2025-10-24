using Microsoft.IdentityModel.Tokens;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;

namespace SubmissionService.Application.Service
{
    public class SubmissionControl
    {
        private string _urlJugde0 = "http://192.168.117.133:2358";// Ubuntu Vmware + judge0 


        private readonly ICompareTestCase _compare;
        private readonly IResultHandle _getResult;
        private readonly ISendToJudge0 _judge0;
        private readonly ISubmissionHandle _submissionHandle;

        public SubmissionControl(
            ICompareTestCase final, 
            IResultHandle getResult, 
            ISendToJudge0 judge0,
            ISubmissionHandle submissionHandle)
        {
            _compare = final;
            _getResult = getResult;
            _judge0 = judge0;
            _submissionHandle = submissionHandle;
        }
        public async Task<bool> Submit(Request request)
        {

            if (request == null) { Console.WriteLine("nguuuuu"); return false; }
            var body = new SubmissionDTO
            {
                AssignmentId = request.AssignmentId,
                Code = request.SourceCode ?? "",
                LanguageId = request.LanguageId,
            };
            // luu bai tap xuong database truoc
            int id = await _submissionHandle.AddSubmission(body);
            if (id < 0) return false;

            // publish assignmentid qua de lay list test case
            request.SubmissionId = id;
            Console.WriteLine("tao da check" + request.SubmissionId);

            await _compare.GetTestCase(request);

            return true;
        }
        public async Task<List<ResultDTO>> GetResults(int studentId, int assignmentId)
        {
            return await _getResult.GetYourResult(studentId, assignmentId);
        }
        public async Task<List<ResultDTO>> GetAllYourSubmissions(int studentId)
        {
            return await _getResult.GetAllYourSubmissions(studentId);
        }

        public async Task<ResultDTO?> SubmitWithResult(string sourceCode, int languageId, string stdin)
        {
            if (sourceCode.IsNullOrEmpty() || languageId<0 ) return null;

            var result = await _judge0.RunCode(sourceCode,languageId,stdin,_urlJugde0);

            if (result == null) return null;    
            return result;
        }

    }
}
