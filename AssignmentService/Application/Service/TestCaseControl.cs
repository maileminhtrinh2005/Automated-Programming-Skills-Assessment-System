using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class TestCaseControl
    {
        private string _submissionUrl = "http://localhost:5090";
        private readonly ITestCaseFunc _testcase;
        public TestCaseControl(ITestCaseFunc testcase)
        {
            _testcase = testcase;
        }


        //public async Task<bool> AddTestCase(string sourcecode, int languageId)
        //{
        //    if (languageId < 0 || sourcecode == null) { return false; }
        //    if (!await _testcase.SaveTestCase(sourcecode, languageId, _submissionUrl)) { return false; }
        //    return true;
        //}
    }
}
