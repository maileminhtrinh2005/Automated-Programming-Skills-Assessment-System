using ShareLibrary;
using ShareLibrary.Event;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using System.Text;

namespace SubmissionService.Infrastructure
{
    public class CompareTestCase : ICompareTestCase
    {

        //private const string _urlJudge0 = "http://192.168.117.133:2358";
        private const string _urlJudge0 = "http://26.215.255.178:2358/";// local judge0
        private readonly IEventBus _eventBus;
        private readonly ISendToJudge0 _judge0;

        private readonly IResultRepository _resultHandle;
        public CompareTestCase(
            IEventBus eventBus,
            IResultRepository resultHandle,
            ISendToJudge0 sendToJudge0)
        {
            _eventBus = eventBus;
            _resultHandle = resultHandle;
            _judge0 = sendToJudge0;
        }

        //gui yeu cau lay testcase 
        public Task GetTestCase(Request request)
        {
            var requestEvent = new CodeSubmittedEvents
            {
                AssignmentId=request.AssignmentId,
                SourceCode=request.SourceCode,
                LanguageId=request.LanguageId,
                SubmissionId=request.SubmissionId
            };
            _eventBus.Publish(requestEvent);
            return Task.CompletedTask;
        }

        public async Task<int> RunAndAddResult(string sourceCode, int languageId, string stdin,int submissionId)
        {
            var resultJudge0 = await _judge0.RunCode(sourceCode, languageId, stdin,_urlJudge0);

            if (resultJudge0 == null) return -1;

            var resultdto = new ResultDTO
            {
                SubmissionId = submissionId,
                TestCaseId = 0,// luu lan dau
                Passed = false,// luu lan dau
                ExecutionTime =resultJudge0.ExecutionTime,
                MemoryUsed = resultJudge0.MemoryUsed,
                Output = resultJudge0.Output,
                ErrorMessage = resultJudge0.ErrorMessage
            };
            int id= await _resultHandle.AddResult(resultdto);
            return id;
        }

        public async Task<double> CompareAndUpdateResult(string resultOutput, string expectedOutput,double weight,int submissionId, int testCaseId, int resultId)
        {
            if (CompareOutput(resultOutput, expectedOutput))
            {

                bool passed = true;

                if (await _resultHandle.UpdateResult(resultId, submissionId, testCaseId, passed))
                {
                    return weight;
                }
                else
                {
                    return -1; // -1 laf bi loix
                }
            }
            else
            {
                bool passed = false;

                if (await _resultHandle.UpdateResult(resultId, submissionId, testCaseId, passed))
                {
                    return weight*0;
                }
                else
                {
                    return -1; // -1 laf bi loix
                }
            }
        }

        private bool CompareOutput(string resultOutput, string expectedOutput)
        {

            string a= NormalizeForComparison(resultOutput)??"";
            string b= NormalizeForComparison(expectedOutput)?? "";

            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
                return true;

            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private string? NormalizeForComparison(string s)
        {
            if (s == null) return null;

            // Chuẩn hóa Unicode
            s = s.Normalize(NormalizationForm.FormC);

            // Chuẩn hoá line endings sang '\n'
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");

            // Loại trailing whitespace trên mỗi dòng (thường do format)
            var lines = s.Split('\n')
                         .Select(line => line.TrimEnd());

            // Loại leading/trailing toàn cục (nếu muốn)
            var joined = string.Join("\n", lines).Trim();

            return joined;
        }
    }
}
