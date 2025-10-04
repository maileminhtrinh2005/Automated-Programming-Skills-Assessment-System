using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;

namespace SubmissionService.Application
{
    public class Sub
    {
        private string _urlJugde0 = "http://192.168.117.133:2358/submissions";// Ubuntu Vmware 
        private readonly ICompareTestCase _final;
        public Sub(ICompareTestCase final)
        {
            _final = final;
        }
        public async Task<bool> Submit(Request request)
        {

            if (request == null) return false;

            if (await _final.FinalResult(request, _urlJugde0) == false)
            {
                return false;
            }

            return true;
        }
    }
}
