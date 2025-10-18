using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;

namespace SubmissionService.Application
{
    public class Sub
    {
        private string _urlJugde0 = "http://192.168.117.133:2358";// Ubuntu Vmware 


        private readonly ICompareTestCase _final;
        private readonly IGetResult _getResult;
        private readonly ISubmit _submit;

        public Sub(ICompareTestCase final, IGetResult getResult,ISubmit submit)
        {
            _final = final;
            _getResult = getResult; 
            _submit = submit;
        }
        public async Task<bool> Submit(Request request)
        {

            if (request == null) { Console.WriteLine("nguuuuu"); return false; }

            if (await _final.FinalResult(request, _urlJugde0) != false)
            {
                return true;
            }
            return false;
        }
        public async Task<List<ResultDTO>> GetResults(int studentId, int assignmentId)
        {
            return await _getResult.GetYourResult(studentId, assignmentId);
        } 
        public async Task<List<ResultDTO>> GetAllYourSubmissions(int studentId)
        {
            return await _getResult.GetAllYourSubmissions(studentId);
        }

        public async Task<ResultDTO> SubmitWithResult(Request request)
        {
            if (request == null) return null;

            var result = await _submit.Submited(request, _urlJugde0);

            return result;
        }

    }
}
